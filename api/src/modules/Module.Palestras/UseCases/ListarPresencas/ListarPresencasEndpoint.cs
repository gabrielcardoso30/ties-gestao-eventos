using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ListarPresencas;

internal sealed class ListarPresencasEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}/presencas", async (Guid id, IUseCase<ListarPresencasRequest, IReadOnlyList<ListarPresencasItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ListarPresencasRequest(id), ct)).ToHttpResult())
            .WithName("ListarPresencas")
            .WithSummary("Lista as presenças registradas na palestra")
            .WithDescription("Retorna as presenças em ordem de registro, com o nome da pessoa (módulo Pessoas) e se o certificado já foi emitido (`certificadoEmitido`).")
            .Produces<IReadOnlyList<ListarPresencasItemResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
