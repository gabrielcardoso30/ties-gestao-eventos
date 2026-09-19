using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.ListarPessoas;

internal sealed class ListarPessoasEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("", async ([AsParameters] ListarPessoasRequest request, IUseCase<ListarPessoasRequest, PagedResult<ListarPessoasItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarPessoas")
            .WithSummary("Lista pessoas (paginado)")
            .WithDescription("""
                Lista pessoas ordenadas por nome, com filtros opcionais:

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `busca` | Texto contido no nome, no e-mail ou na empresa (case-insensitive, até 100 caracteres) |
                | `estaAtivo` | `true`/`false` |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |
                | `ordenarPor`, `direcao` | Campo permitido e direção `Asc`/`Desc` |

                Pessoas excluídas nunca são retornadas. **Perfil exigido:** qualquer usuário autenticado.
                """)
            .WithValidation<ListarPessoasRequest>()
            .Produces<PagedResult<ListarPessoasItemResponse>>();
}
