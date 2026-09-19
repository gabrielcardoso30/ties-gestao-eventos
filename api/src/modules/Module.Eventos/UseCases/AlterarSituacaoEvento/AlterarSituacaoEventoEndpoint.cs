using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.AlterarSituacaoEvento;

internal sealed class AlterarSituacaoEventoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPatch("/{id:guid}/situacao", async (Guid id, AlterarSituacaoEventoRequest request, IUseCase<AlterarSituacaoEventoRequest, AlterarSituacaoEventoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { EventoId = id }, ct)).ToHttpResult())
            .WithName("AlterarSituacaoEvento")
            .WithSummary("Altera a situação do evento (publicar, iniciar, encerrar, cancelar)")
            .WithDescription("""
                Aplica uma transição na máquina de estados do evento.

                | De | Para | Regra |
                |----|------|-------|
                | `Rascunho` | `Publicado` | Exige ao menos uma palestra cadastrada (`422 Eventos.EventoSemPalestras`). Emite `EventoPublicado`. |
                | `Publicado` | `EmAndamento` | — |
                | `Publicado`, `EmAndamento` | `Encerrado` | — |
                | `Rascunho`, `Publicado`, `EmAndamento` | `Cancelado` | `motivo` obrigatório (`422 Eventos.MotivoCancelamentoObrigatorio`). Emite `EventoCancelado`. |

                Qualquer outra combinação (inclusive voltar para `Rascunho`) responde `422 Eventos.TransicaoSituacaoInvalida`.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AlterarSituacaoEventoRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AlterarSituacaoEventoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
