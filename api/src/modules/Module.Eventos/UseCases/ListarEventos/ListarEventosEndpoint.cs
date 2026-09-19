using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.ListarEventos;

internal sealed class ListarEventosEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("", async ([AsParameters] ListarEventosRequest request, IUseCase<ListarEventosRequest, PagedResult<ListarEventosItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarEventos")
            .WithSummary("Lista eventos (paginado)")
            .WithDescription("""
                Lista eventos ordenados por data de início (mais recentes primeiro), com filtros opcionais:

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `busca` | Texto contido no nome ou na descrição (case-insensitive) |
                | `eventoSituacao` | `Rascunho`, `Publicado`, `EmAndamento`, `Encerrado` ou `Cancelado` |
                | `eventoFormato` | `Presencial`, `Remoto` ou `Hibrido` |
                | `dataInicioDe`, `dataInicioAte` | Intervalo (ISO-8601) aplicado sobre `eventoDataInicio` |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |
                | `ordenarPor`, `direcao` | Campo permitido e direção `Asc`/`Desc` |

                Cada item traz `inscricoesConfirmadas`. **Perfil exigido:** qualquer usuário autenticado.
                """)
            .WithValidation<ListarEventosRequest>()
            .Produces<PagedResult<ListarEventosItemResponse>>();
}
