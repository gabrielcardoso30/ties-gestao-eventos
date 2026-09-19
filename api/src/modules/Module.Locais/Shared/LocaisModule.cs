using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Locais;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.WebHost;
using Shared.WebHost.Modules;

namespace Module.Locais.Shared;

/// <summary>Ponto de entrada do módulo Locais: serviços, contrato público e endpoints agrupados em <c>api/v1/locais</c>.</summary>
public sealed class LocaisModule : IModule
{
    public string Name => LocaisDbContext.SchemaName;
    public string RoutePrefix => "locais";
    public string Description => """
        Locais onde eventos presenciais acontecem e suas **salas** (ambientes).

        - Um local possui uma ou mais salas; um local de ambiente único nasce com a sala *"Ambiente único"*.
        - Salas são a unidade alocável para palestras (módulo Palestras consulta este módulo via `ILocaisModuleApi`).
        - Exclusões são lógicas (soft delete) e toda alteração gera trilha de auditoria.
        """;

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<LocaisDbContext>(LocaisDbContext.SchemaName);
        builder.Services.AddScoped<ILocaisModuleApi, LocaisModuleApi>();
        builder.Services.AddUseCasesFromAssembly(typeof(LocaisModule).Assembly, LocaisTelemetry.Instance);
        builder.Services.AddModuleValidators(typeof(LocaisModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapModuleGroup(RoutePrefix, Name).MapEndpointsFromAssembly(typeof(LocaisModule).Assembly);
}
