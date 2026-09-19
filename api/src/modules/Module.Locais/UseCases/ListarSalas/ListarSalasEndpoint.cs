using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.ListarSalas;

internal sealed class ListarSalasEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}/salas", async ([AsParameters] ListarSalasRequest request, IUseCase<ListarSalasRequest, IReadOnlyList<ListarSalasItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarSalas")
            .WithSummary("Lista as salas de um local")
            .WithDescription("Salas ordenadas por nome. Use `estaAtivo=true` para obter apenas salas disponíveis para alocação.")
            .WithValidation<ListarSalasRequest>()
            .Produces<IReadOnlyList<ListarSalasItemResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
