using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.ObterUsuarioAtual;

internal sealed class ObterUsuarioAtualEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/usuarios/me", async (ICurrentUser currentUser, IUseCase<ObterUsuarioAtualRequest, ObterUsuarioAtualResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ObterUsuarioAtualRequest(currentUser.Id ?? Guid.Empty), ct)).ToHttpResult())
            .WithName("ObterUsuarioAtual")
            .WithSummary("Obtém o usuário autenticado (perfil próprio)")
            .WithDescription("Retorna os dados do usuário dono do token (claim `sub`): nome, e-mail, perfis e último acesso. **Qualquer usuário autenticado.**")
            .Produces<ObterUsuarioAtualResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
