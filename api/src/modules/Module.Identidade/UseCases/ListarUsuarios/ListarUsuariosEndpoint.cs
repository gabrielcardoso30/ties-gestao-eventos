using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.ListarUsuarios;

internal sealed class ListarUsuariosEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/usuarios", async ([AsParameters] ListarUsuariosRequest request, IUseCase<ListarUsuariosRequest, PagedResult<ListarUsuariosItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarUsuarios")
            .WithSummary("Lista usuários (paginado)")
            .WithDescription("""
                Lista usuários ordenados por nome, com seus perfis e último acesso. Filtros opcionais:

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `busca` | Texto contido no nome ou no e-mail (case-insensitive) |
                | `estaAtivo` | `true`/`false` |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |

                **Perfil exigido:** Administrador.
                """)
            .WithValidation<ListarUsuariosRequest>()
            .RequireAuthorization(Politicas.Administracao)
            .Produces<PagedResult<ListarUsuariosItemResponse>>();
}
