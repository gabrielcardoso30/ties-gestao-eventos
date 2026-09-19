using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Shared.Observability.Middleware;
using Shared.Observability.Telemetria;

namespace Shared.Observability;

/// <summary>
/// Observabilidade como pilar: logs estruturados (Serilog) correlacionados com traces (OpenTelemetry),
/// métricas de runtime/HTTP/EF/Npgsql e de negócio (por módulo). Exporta via OTLP (VPS: Aspire Dashboard, Grafana,
/// Jaeger) e/ou Azure Application Insights, escolhido apenas por configuração.
/// </summary>
public static class ObservabilityExtensions
{
    public static IHostApplicationBuilder AddObservability(this IHostApplicationBuilder builder, string serviceName)
    {
        var versao = typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var appInsights = builder.Configuration["ApplicationInsights:ConnectionString"];

        builder.Services.AddSerilog((services, configuracao) =>
        {
            configuracao
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", serviceName)
                .Enrich.WithProperty("Version", versao);

            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            {
                configuracao.WriteTo.OpenTelemetry(o =>
                {
                    o.Endpoint = otlpEndpoint;
                    o.ResourceAttributes = new Dictionary<string, object> { ["service.name"] = serviceName, ["service.version"] = versao };
                });
            }
        });

        var otel = builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName, serviceVersion: versao, serviceInstanceId: Environment.MachineName)
                .AddAttributes([new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)]))
            .WithTracing(t => t
                .AddSource($"{ModuleTelemetry.Prefixo}.*")
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health") && !ctx.Request.Path.StartsWithSegments("/swagger");
                    o.EnrichWithHttpRequest = (activity, request) =>
                    {
                        if (request.HttpContext.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var cid))
                        {
                            activity.SetTag("correlation.id", cid);
                        }
                    };
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddNpgsql())
            .WithMetrics(m => m
                .AddMeter($"{ModuleTelemetry.Prefixo}.*")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddNpgsqlInstrumentation());

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            otel.UseOtlpExporter();
        }

        if (!string.IsNullOrWhiteSpace(appInsights))
        {
            otel.UseAzureMonitor(o => o.ConnectionString = appInsights);
        }

        return builder;
    }

    /// <summary>Middlewares de observabilidade: correlação e log de requisição enriquecido (usuário, IP, trace).</summary>
    public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging(o =>
        {
            o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} => {StatusCode} em {Elapsed:0.0} ms";
            o.GetLevel = (ctx, _, ex) => ex is not null || ctx.Response.StatusCode >= 500 ? LogEventLevel.Error
                : ctx.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
                : LogEventLevel.Information;
            o.EnrichDiagnosticContext = (diag, ctx) =>
            {
                diag.Set("ClientIp", ctx.Connection.RemoteIpAddress?.ToString());
                diag.Set("UserAgent", ctx.Request.Headers.UserAgent.ToString());
                diag.Set("UsuarioId", ctx.User.FindFirst("sub")?.Value ?? ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                diag.Set("TraceId", Activity.Current?.TraceId.ToString());
                if (ctx.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var cid))
                {
                    diag.Set("CorrelationId", cid);
                }
            };
        });
        return app;
    }
}
