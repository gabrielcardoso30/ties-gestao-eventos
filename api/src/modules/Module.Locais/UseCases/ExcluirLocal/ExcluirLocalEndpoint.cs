using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ExcluirLocal;

internal sealed class ExcluirLocalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}", async (Guid id, IUseCase<ExcluirLocalRequest, ExcluirLocalResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ExcluirLocalRequest(id), ct)).ToNoContentResult())
            .WithName("ExcluirLocal")
            .WithSummary("Exclui (logicamente) um local")
            .WithDescription("Soft delete do local e de suas salas: registros permanecem no banco com `ExcluidoEm`/`ExcluidoPor` preenchidos. **Perfil exigido:** Administrador ou Organizador.")
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
