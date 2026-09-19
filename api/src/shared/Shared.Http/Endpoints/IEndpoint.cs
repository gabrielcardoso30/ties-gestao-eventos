using Microsoft.AspNetCore.Routing;

namespace Shared.Http.Endpoints;

/// <summary>
/// Cada caso de uso expõe exatamente um endpoint, mapeado dentro do grupo do módulo
/// (<c>api/v1/nome-modulo</c>), garantindo um único conjunto por módulo no Swagger.
/// </summary>
public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder group);
}
