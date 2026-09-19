using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RemoverConteudo;

internal sealed class RemoverConteudoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}/conteudos/{conteudoId:guid}", async (Guid id, Guid conteudoId, IUseCase<RemoverConteudoRequest, RemoverConteudoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new RemoverConteudoRequest(id, conteudoId), ct)).ToNoContentResult())
            .WithName("RemoverConteudo")
            .WithSummary("Remove (logicamente) um conteúdo da palestra")
            .WithDescription("Soft delete do conteúdo. Conteúdo inexistente na palestra: `404 Palestras.ConteudoNaoEncontrado`. **Perfil exigido:** Administrador ou Organizador.")
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
