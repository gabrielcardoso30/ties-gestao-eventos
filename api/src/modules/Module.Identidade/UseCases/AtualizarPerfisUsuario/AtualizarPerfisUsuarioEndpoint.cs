using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.AtualizarPerfisUsuario;

internal sealed class AtualizarPerfisUsuarioEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/usuarios/{id:guid}/perfis", async (Guid id, AtualizarPerfisUsuarioRequest request, IUseCase<AtualizarPerfisUsuarioRequest, AtualizarPerfisUsuarioResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { UsuarioId = id }, ct)).ToHttpResult())
            .WithName("AtualizarPerfisUsuario")
            .WithSummary("Substitui os perfis de um usuário")
            .WithDescription("""
                Define o conjunto **completo** de perfis do usuário: perfis ausentes na lista são removidos e os novos, adicionados.
                Perfis aceitos: `Administrador`, `Organizador`, `Participante` (`422 Identidade.PerfilInvalido`). Tokens já emitidos mantêm os perfis antigos até expirarem.

                **Perfil exigido:** Administrador.
                """)
            .WithValidation<AtualizarPerfisUsuarioRequest>()
            .RequireAuthorization(Politicas.Administracao)
            .Produces<AtualizarPerfisUsuarioResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
