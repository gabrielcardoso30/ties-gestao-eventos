using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Module.Identidade.Domain;
using Module.Identidade.Shared.Seed;
using Module.Identidade.Shared.Seguranca;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.WebHost;
using Shared.WebHost.Modules;

namespace Module.Identidade.Shared;

/// <summary>Ponto de entrada do módulo Identidade: Identity Core + JWT, seed e endpoints agrupados em <c>api/v1/identidade</c>.</summary>
public sealed class IdentidadeModule : IModule
{
    /// <summary>Política de rate limiting do login (por IP), mais restritiva que a global.</summary>
    public const string PoliticaRateLimitLogin = "login";

    public string Name => IdentidadeDbContext.SchemaName;
    public string RoutePrefix => "identidade";
    public string Description => """
        Autenticação e usuários da aplicação (ASP.NET Core Identity + **JWT Bearer**).

        - `POST /sessoes` autentica por e-mail/senha e devolve o token; use-o no cabeçalho `Authorization: Bearer {token}`.
        - Perfis: `Administrador`, `Organizador` e `Participante`. Escrita nos demais módulos exige Administrador ou Organizador; administração de usuários exige Administrador.
        - Política de senha: mínimo 8 caracteres com maiúscula, minúscula, dígito e símbolo. Após **5** tentativas inválidas o usuário fica bloqueado por **5 minutos**.
        - Login registra `UsuarioAutenticado`; registro de usuário registra `UsuarioRegistrado` (Outbox). Toda alteração gera trilha de auditoria (sem segredos).
        """;

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<IdentidadeDbContext>(IdentidadeDbContext.SchemaName);

        builder.Services
            .AddIdentityCore<Usuario>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireUppercase = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireDigit = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Lockout.AllowedForNewUsers = true;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                o.User.RequireUniqueEmail = true;
            })
            .AddRoles<Perfil>()
            .AddEntityFrameworkStores<IdentidadeDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<IdentidadeErrorDescriber>();

        builder.Services.Configure<AdministradorInicialOptions>(builder.Configuration.GetSection(AdministradorInicialOptions.Secao));
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddHostedService<IdentidadeSeedHostedService>();

        builder.Services.Configure<RateLimiterOptions>(o => o.AddPolicy(PoliticaRateLimitLogin, ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })));

        builder.Services.AddUseCasesFromAssembly(typeof(IdentidadeModule).Assembly, IdentidadeTelemetry.Instance);
        builder.Services.AddModuleValidators(typeof(IdentidadeModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapModuleGroup(RoutePrefix, Name).MapEndpointsFromAssembly(typeof(IdentidadeModule).Assembly);
}
