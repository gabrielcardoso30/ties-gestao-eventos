using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.ExcluirTrilha;
internal sealed class ExcluirTrilhaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) => group.MapDelete("/{id:guid}/trilhas/{trilhaId:guid}", async (Guid id, Guid trilhaId, IUseCase<ExcluirTrilhaRequest, ExcluirTrilhaResponse> useCase, CancellationToken ct) => (await useCase.HandleAsync(new(id, trilhaId), ct)).ToNoContentResult())
        .WithName("ExcluirTrilha").WithSummary("Exclui logicamente uma trilha").WithDescription("O evento deve manter ao menos uma trilha. **Perfil exigido:** Administrador ou Organizador.").RequireAuthorization(Politicas.Gestao).Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
