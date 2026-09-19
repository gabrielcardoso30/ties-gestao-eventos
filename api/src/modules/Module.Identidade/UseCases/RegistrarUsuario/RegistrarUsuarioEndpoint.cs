using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.RegistrarUsuario;

internal sealed class RegistrarUsuarioEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/usuarios", async (RegistrarUsuarioRequest request, IUseCase<RegistrarUsuarioRequest, RegistrarUsuarioResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToCreatedResult(r => $"/api/v1/identidade/usuarios/{r.Id}"))
            .WithName("RegistrarUsuario")
            .WithSummary("Registra um usuário com seus perfis")
            .WithDescription("""
                Cria um usuário (o e-mail é também o login) e o vincula aos perfis informados.

                - Perfis aceitos: `Administrador`, `Organizador`, `Participante` (`422 Identidade.PerfilInvalido` para qualquer outro).
                - Política de senha: mínimo 8 caracteres, com maiúscula, minúscula, dígito e símbolo (`422 Identidade.SenhaFraca`, com os motivos).
                - E-mail único (`409 Identidade.EmailJaCadastrado`).
                - Registra o evento `UsuarioRegistrado` no Outbox na mesma transação.

                **Perfil exigido:** Administrador.
                """)
            .WithValidation<RegistrarUsuarioRequest>()
            .RequireAuthorization(Politicas.Administracao)
            .Produces<RegistrarUsuarioResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
