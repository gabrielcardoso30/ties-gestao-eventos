using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Eventos;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.WebHost;
using Shared.WebHost.Modules;

namespace Module.Eventos.Shared;

/// <summary>Ponto de entrada do módulo Eventos: serviços, contrato público e endpoints agrupados em <c>api/v1/eventos</c>.</summary>
public sealed class EventosModule : IModule
{
    public string Name => EventosDbContext.SchemaName;
    public string RoutePrefix => "eventos";
    public string Description => """
        Eventos (presenciais, remotos ou híbridos) e as **inscrições** de participantes.

        - Um evento nasce em `Rascunho` e segue o ciclo `Publicado` → `EmAndamento` → `Encerrado`, podendo ser `Cancelado` antes de encerrar.
        - Publicar exige ao menos uma palestra (consulta ao módulo Palestras via `IPalestrasModuleApi`).
        - Eventos presenciais/híbridos referenciam um local do módulo Locais (`ILocaisModuleApi`); a capacidade do evento, quando não informada, é a do local.
        - Inscrições referenciam pessoas do módulo Pessoas (`IPessoasModuleApi`); o cancelamento mantém o registro com situação `Cancelada`.
        - Emite os eventos de integração `EventoPublicado`, `EventoCancelado`, `InscricaoRealizada` e `InscricaoCancelada` via Outbox.
        """;

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<EventosDbContext>(EventosDbContext.SchemaName);
        builder.Services.AddScoped<IEventosModuleApi, EventosModuleApi>();
        builder.Services.AddUseCasesFromAssembly(typeof(EventosModule).Assembly, EventosTelemetry.Instance);
        builder.Services.AddModuleValidators(typeof(EventosModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapModuleGroup(RoutePrefix, Name).MapEndpointsFromAssembly(typeof(EventosModule).Assembly);
}
