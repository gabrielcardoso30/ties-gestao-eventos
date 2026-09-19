using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Auditoria.UseCases.ListarRegistrosAuditoria;

internal sealed class ListarRegistrosAuditoriaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/registros", async ([AsParameters] ListarRegistrosAuditoriaRequest request, IUseCase<ListarRegistrosAuditoriaRequest, PagedResult<ListarRegistrosAuditoriaItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarRegistrosAuditoria")
            .WithSummary("Lista registros de auditoria (paginado)")
            .WithDescription("""
                Consulta a trilha de auditoria, do mais recente para o mais antigo (`ocorridoEm` desc). Filtros opcionais, combinados com AND:

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `modulo` | Módulo de origem (ex.: `Locais`, `Pessoas`) |
                | `entidadeNome` | Nome da entidade (ex.: `Local`, `Sala`, `Pessoa`) |
                | `entidadeId` | Identificador da entidade (texto; normalmente um Guid) |
                | `usuarioId` | Guid do usuário que fez a alteração |
                | `operacao` | `Inclusao`, `Alteracao` ou `Exclusao` |
                | `ocorridoDe`, `ocorridoAte` | Intervalo (ISO-8601, inclusivo) do momento da alteração |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |
                | `ordenarPor`, `direcao` | Campo permitido e direção `Asc`/`Desc` |

                A listagem não traz os dados antes/depois; use `GET /api/v1/auditoria/registros/{id}` para o detalhe.
                **Perfil exigido:** Administrador.
                """)
            .WithValidation<ListarRegistrosAuditoriaRequest>()
            .RequireAuthorization(Politicas.Administracao)
            .Produces<PagedResult<ListarRegistrosAuditoriaItemResponse>>();
}
