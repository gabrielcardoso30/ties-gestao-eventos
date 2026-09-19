using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Shared.WebHost.Modules;

/// <summary>
/// Contrato de um módulo do monolito. O host descobre as implementações (assemblies <c>Module.*.dll</c>),
/// chama <see cref="ConfigureServices"/> na composição e <see cref="MapEndpoints"/> no pipeline.
/// Um módulo só conhece Shared.* e Shared.Contracts; nunca outro módulo diretamente.
/// </summary>
public interface IModule
{
    /// <summary>Nome do módulo, igual ao schema no banco e à tag no Swagger (ex.: "Locais").</summary>
    string Name { get; }

    /// <summary>Segmento de rota após <c>api/v1/</c> (ex.: "locais").</summary>
    string RoutePrefix { get; }

    /// <summary>Descrição em markdown exibida no Swagger para a tag do módulo.</summary>
    string Description { get; }

    void ConfigureServices(IHostApplicationBuilder builder);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
