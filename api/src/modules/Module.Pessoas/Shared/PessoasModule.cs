using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Pessoas;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.WebHost;
using Shared.WebHost.Modules;

namespace Module.Pessoas.Shared;

/// <summary>Ponto de entrada do módulo Pessoas: serviços, contrato público e endpoints agrupados em <c>api/v1/pessoas</c>.</summary>
public sealed class PessoasModule : IModule
{
    public string Name => PessoasDbContext.SchemaName;
    public string RoutePrefix => "pessoas";
    public string Description => """
        Cadastro único de **pessoas**: palestrantes e participantes são Pessoas; o papel nasce do relacionamento com eventos e palestras.

        - E-mail é único entre pessoas ativas e armazenado em minúsculas; o CPF é armazenado apenas com dígitos e validado (dígitos verificadores).
        - Outros módulos consultam este via `IPessoasModuleApi` (resumo mínimo: id, nome e e-mail, apenas pessoas ativas).
        - Eventos de integração publicados: `PessoaCriada` e `PessoaExcluida`.
        - Exclusões são lógicas (soft delete) e toda alteração gera trilha de auditoria.
        """;

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<PessoasDbContext>(PessoasDbContext.SchemaName);
        builder.Services.AddScoped<IPessoasModuleApi, PessoasModuleApi>();
        builder.Services.AddUseCasesFromAssembly(typeof(PessoasModule).Assembly, PessoasTelemetry.Instance);
        builder.Services.AddModuleValidators(typeof(PessoasModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapModuleGroup(RoutePrefix, Name).MapEndpointsFromAssembly(typeof(PessoasModule).Assembly);
}
