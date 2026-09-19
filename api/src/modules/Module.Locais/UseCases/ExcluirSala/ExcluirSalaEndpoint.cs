using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ExcluirSala;

internal sealed class ExcluirSalaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}/salas/{salaId:guid}", async (Guid id, Guid salaId, IUseCase<ExcluirSalaRequest, ExcluirSalaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ExcluirSalaRequest(id, salaId), ct)).ToNoContentResult())
            .WithName("ExcluirSala")
            .WithSummary("Exclui (logicamente) uma sala")
            .WithDescription("Um local precisa manter ao menos uma sala (`422 Locais.LocalPrecisaDeUmaSala`). **Perfil exigido:** Administrador ou Organizador.")
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
