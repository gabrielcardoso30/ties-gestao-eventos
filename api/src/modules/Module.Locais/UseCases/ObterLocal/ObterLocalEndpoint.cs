using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ObterLocal;

internal sealed class ObterLocalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}", async (Guid id, IUseCase<ObterLocalRequest, ObterLocalResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ObterLocalRequest(id), ct)).ToHttpResult())
            .WithName("ObterLocal")
            .WithSummary("Obtém um local com suas salas")
            .WithDescription("Retorna o local, endereço, capacidade total (soma das salas ativas) e a lista de salas.")
            .Produces<ObterLocalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
