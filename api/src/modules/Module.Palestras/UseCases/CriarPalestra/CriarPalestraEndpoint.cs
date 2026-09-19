using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.CriarPalestra;

internal sealed class CriarPalestraEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("", async (CriarPalestraRequest request, IUseCase<CriarPalestraRequest, CriarPalestraResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToCreatedResult(r => $"/api/v1/palestras/{r.Id}"))
            .WithName("CriarPalestra")
            .WithSummary("Cria uma palestra em um evento")
            .WithDescription("""
                Cria uma palestra vinculada a um evento, com ao menos um palestrante.

                **Regras validadas com outros módulos:**

                | Regra | Erro |
                |-------|------|
                | Evento existe | `422 Palestras.EventoNaoEncontrado` |
                | Evento não está `Encerrado`/`Cancelado` | `422 Palestras.EventoNaoAceitaPalestras` |
                | Período da palestra dentro do período do evento | `422 Palestras.PeriodoForaDoEvento` |
                | `salaId` (opcional) existe e está ativa | `422 Palestras.SalaNaoEncontrada` |
                | Sala pertence ao local do evento | `422 Palestras.SalaNaoPertenceAoLocal` |
                | Sem outra palestra na mesma sala no horário | `409 Palestras.SalaOcupada` |
                | Todas as pessoas informadas existem | `422 Palestras.PessoaNaoEncontrada` |
                | Pessoa não repetida entre os palestrantes | `409 Palestras.PalestranteJaVinculado` |

                Papéis de palestrante: `Principal`, `Coautor`, `Mediador`. Emite o evento de integração `PalestraCriada`.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<CriarPalestraRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<CriarPalestraResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
