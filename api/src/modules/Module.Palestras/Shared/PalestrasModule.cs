using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Palestras;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.WebHost;
using Shared.WebHost.Modules;

namespace Module.Palestras.Shared;

/// <summary>Ponto de entrada do módulo Palestras: serviços, contrato público e endpoints agrupados em <c>api/v1/palestras</c>.</summary>
public sealed class PalestrasModule : IModule
{
    public string Name => PalestrasDbContext.SchemaName;
    public string RoutePrefix => "palestras";
    public string Description => """
        Palestras de um evento, seus **palestrantes**, **conteúdos**, **presenças** e **certificados**.

        - Uma palestra pertence a um evento (módulo Eventos) e pode ocupar uma sala do local do evento (módulo Locais); a agenda da sala não admite sobreposição.
        - Palestrantes e participantes são pessoas (módulo Pessoas); a palestra mantém sempre ao menos um palestrante.
        - Presença exige inscrição confirmada no evento; o certificado exige presença e palestra encerrada, e é validável publicamente pelo código.
        - Exclusões são lógicas (soft delete) e toda alteração gera trilha de auditoria.
        """;

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<PalestrasDbContext>(PalestrasDbContext.SchemaName);
        builder.Services.AddScoped<IPalestrasModuleApi, PalestrasModuleApi>();
        builder.Services.AddScoped<AgendaPalestraVerificador>();
        builder.Services.AddUseCasesFromAssembly(typeof(PalestrasModule).Assembly, PalestrasTelemetry.Instance);
        builder.Services.AddModuleValidators(typeof(PalestrasModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapModuleGroup(RoutePrefix, Name).MapEndpointsFromAssembly(typeof(PalestrasModule).Assembly);
}
