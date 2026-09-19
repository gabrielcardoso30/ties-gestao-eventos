using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Module.Identidade.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.CriarSessao;

internal sealed class CriarSessaoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/sessoes", async (CriarSessaoRequest request, IUseCase<CriarSessaoRequest, CriarSessaoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("CriarSessao")
            .WithSummary("Autentica um usuário e emite o token de acesso (login)")
            .WithDescription("""
                Autentica por **e-mail e senha** e devolve um JWT Bearer (HS256) com as claims `sub`, `name`, `email`, `role` (uma por perfil) e `jti`.
                Envie-o nas demais chamadas em `Authorization: Bearer {accessToken}`; `expiraEm` informa a validade.

                | Situação | Resposta |
                |----------|----------|
                | E-mail inexistente ou senha incorreta | `401 Identidade.CredenciaisInvalidas` |
                | 5 tentativas inválidas seguidas | `401 Identidade.UsuarioBloqueado` (bloqueio por 5 minutos) |
                | Usuário desativado | `401 Identidade.UsuarioInativo` |
                | Mais de 10 tentativas por minuto no mesmo IP | `429 Too Many Requests` |

                O login bem-sucedido atualiza `ultimoAcessoEm` e registra o evento `UsuarioAutenticado`. **Endpoint anônimo.**
                """)
            .WithValidation<CriarSessaoRequest>()
            .AllowAnonymous()
            .RequireRateLimiting(IdentidadeModule.PoliticaRateLimitLogin)
            .Produces<CriarSessaoResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
}
