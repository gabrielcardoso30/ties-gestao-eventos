using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.ListarInscricoes;

internal sealed class ListarInscricoesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}/inscricoes", async (Guid id, [AsParameters] ListarInscricoesRequest request, IUseCase<ListarInscricoesRequest, PagedResult<ListarInscricoesItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { EventoId = id }, ct)).ToHttpResult())
            .WithName("ListarInscricoes")
            .WithSummary("Lista as inscrições de um evento (paginado)")
            .WithDescription("""
                Lista inscrições ordenadas pela data de realização, com nome e e-mail obtidos do módulo Pessoas em lote.

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `inscricaoSituacao` | `Confirmada` ou `Cancelada` (omitido: todas) |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |

                **Perfil exigido:** qualquer usuário autenticado.
                """)
            .WithValidation<ListarInscricoesRequest>()
            .Produces<PagedResult<ListarInscricoesItemResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
