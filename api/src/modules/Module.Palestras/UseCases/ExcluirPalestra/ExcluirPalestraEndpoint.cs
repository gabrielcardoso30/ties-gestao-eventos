using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ExcluirPalestra;

internal sealed class ExcluirPalestraEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}", async (Guid id, IUseCase<ExcluirPalestraRequest, ExcluirPalestraResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ExcluirPalestraRequest(id), ct)).ToNoContentResult())
            .WithName("ExcluirPalestra")
            .WithSummary("Exclui (logicamente) uma palestra")
            .WithDescription("""
                Soft delete da palestra e, em cascata lógica, de seus **palestrantes** e **conteúdos** (`ExcluidoEm`/`ExcluidoPor` preenchidos).

                **Presenças e certificados permanecem**: certificados já emitidos continuam validáveis em `GET /api/v1/palestras/certificados/{codigo}`.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
