using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.CancelarInscricao;

internal sealed class CancelarInscricaoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}/inscricoes/{inscricaoId:guid}", async (Guid id, Guid inscricaoId, IUseCase<CancelarInscricaoRequest, CancelarInscricaoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new CancelarInscricaoRequest(id, inscricaoId), ct)).ToNoContentResult())
            .WithName("CancelarInscricao")
            .WithSummary("Cancela uma inscrição")
            .WithDescription("""
                Muda a situação da inscrição para **Cancelada** (não é exclusão lógica) e emite `InscricaoCancelada`.

                - Inscrição inexistente no evento: `404 Eventos.InscricaoNaoEncontrada`.
                - Inscrição já cancelada: `422 Eventos.InscricaoJaCancelada`.

                **Perfil exigido:** qualquer usuário autenticado.
                """)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
