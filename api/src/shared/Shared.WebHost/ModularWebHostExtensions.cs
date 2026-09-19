using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Common;
using Shared.Contracts.Identidade;
using Shared.Data;
using Shared.Messaging;
using Shared.Observability;
using Shared.WebHost.Middleware;
using Shared.WebHost.Modules;
using Shared.WebHost.OpenApi;
using Shared.WebHost.Security;

namespace Shared.WebHost;

/// <summary>
/// Composição do host: tudo o que é transversal (observabilidade, segurança, dados, mensageria, OpenAPI) fica aqui,
/// e o <c>Program.cs</c> do host tem 5 linhas. Um futuro Host.Worker reutiliza as mesmas peças.
/// </summary>
public static class ModularWebHostExtensions
{
    public const string ModulesKey = "GestaoEventos.Modules";

    public static IHostApplicationBuilder AddModularWebHost(this WebApplicationBuilder builder, string serviceName)
    {
        var modules = ModuleDiscovery.Discover();
        builder.Services.AddSingleton<IReadOnlyList<IModule>>(modules);

        builder.AddObservability(serviceName);
        builder.AddSharedData();
        builder.AddSharedMessaging();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICurrentUser, CurrentUser>();
        builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Extensions.TryAdd("traceId", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? ctx.HttpContext.TraceIdentifier);
            ctx.ProblemDetails.Instance ??= ctx.HttpContext.Request.Path;
        });
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            o.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddOpenApi("v1", o =>
        {
            o.AddDocumentTransformer((doc, ctx, ct) => new DocumentoTransformer(ctx.ApplicationServices.GetRequiredService<IReadOnlyList<IModule>>()).TransformAsync(doc, ctx, ct));
            o.AddOperationTransformer<SegurancaOperationTransformer>();
        });

        AdicionarSeguranca(builder);
        AdicionarRateLimiting(builder);
        AdicionarCors(builder);
        AdicionarHealthChecks(builder);

        builder.Services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            o.KnownNetworks.Clear();
            o.KnownProxies.Clear();
        });

        foreach (var module in modules)
        {
            module.ConfigureServices(builder);
        }

        return builder;
    }

    public static WebApplication UseModularWebHost(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();

        app.UseForwardedHeaders();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseObservability();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
        app.MapOpenApi("/openapi/{documentName}.yaml").AllowAnonymous();
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/openapi/v1.json", "Gestão de Eventos v1");
            o.DocumentTitle = "Gestão de Eventos — API";
            o.DisplayRequestDuration();
            o.EnablePersistAuthorization();
            o.DefaultModelsExpandDepth(1);
        });
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription().AllowAnonymous();

        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") }).AllowAnonymous();

        foreach (var module in modules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }

    private static void AdicionarSeguranca(WebApplicationBuilder builder)
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Secao));
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey deve ter ao menos 32 caracteres (configure por variável de ambiente Jwt__SigningKey).");
        }

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };
                o.MapInboundClaims = true;
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Politicas.Administracao, p => p.RequireRole(PerfisPadrao.Administrador))
            .AddPolicy(Politicas.Gestao, p => p.RequireRole(PerfisPadrao.Administrador, PerfisPadrao.Organizador));
    }

    private static void AdicionarRateLimiting(WebApplicationBuilder builder)
    {
        var limite = builder.Configuration.GetValue("RateLimiting:PermitLimit", 300);
        var janela = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);
        builder.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ctx.User.Identity?.Name ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = limite, Window = TimeSpan.FromSeconds(janela), QueueLimit = 0 }));
            o.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.ContentType = "application/problem+json";
                await ctx.HttpContext.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Muitas requisições",
                    Detail = "Limite de requisições excedido. Tente novamente em instantes.",
                    Type = Shared.Http.Results.ResultHttpExtensions.ProblemTypeBase + "LimiteRequisicoes",
                }, ct);
            };
        });
    }

    private static void AdicionarCors(WebApplicationBuilder builder)
    {
        var origens = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
        builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
            .WithOrigins(origens)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Correlation-Id", "Location")));
    }

    private static void AdicionarHealthChecks(WebApplicationBuilder builder)
    {
        var cs = builder.Configuration.GetConnectionString(DataServiceCollectionExtensions.ConnectionStringName) ?? string.Empty;
        builder.Services.AddHealthChecks()
            .AddNpgSql(cs, name: "postgres", tags: ["ready"], failureStatus: HealthStatus.Unhealthy);
    }

    /// <summary>Atalho para módulos: registra validators FluentValidation do assembly.</summary>
    public static IServiceCollection AddModuleValidators(this IServiceCollection services, System.Reflection.Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        return services;
    }
}
