using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RemoverPalestrante;

internal sealed class RemoverPalestranteEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}/palestrantes/{pessoaId:guid}", async (Guid id, Guid pessoaId, IUseCase<RemoverPalestranteRequest, RemoverPalestranteResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new RemoverPalestranteRequest(id, pessoaId), ct)).ToNoContentResult())
            .WithName("RemoverPalestrante")
            .WithSummary("Remove (logicamente) um palestrante da palestra")
            .WithDescription("""
                Desvincula a pessoa da palestra (soft delete do vínculo).

                - Vínculo inexistente: `404 Palestras.PalestranteNaoEncontrado`.
                - Uma palestra precisa manter ao menos um palestrante: `422 Palestras.PalestraPrecisaDePalestrante`.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
