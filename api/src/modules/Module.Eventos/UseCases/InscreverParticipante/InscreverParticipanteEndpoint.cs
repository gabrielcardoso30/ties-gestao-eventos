using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.InscreverParticipante;

internal sealed class InscreverParticipanteEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id:guid}/inscricoes", async (Guid id, InscreverParticipanteRequest request, IUseCase<InscreverParticipanteRequest, InscreverParticipanteResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { EventoId = id }, ct)).ToCreatedResult(r => $"/api/v1/eventos/{r.EventoId}/inscricoes/{r.Id}"))
            .WithName("InscreverParticipante")
            .WithSummary("Inscreve uma pessoa em um evento")
            .WithDescription("""
                Cria uma inscrição **Confirmada** e emite `InscricaoRealizada`.

                | Regra | Erro |
                |-------|------|
                | Evento deve estar `Publicado` ou `EmAndamento` | `422 Eventos.EventoNaoAceitaInscricoes` |
                | Pessoa deve existir no módulo Pessoas | `422 Eventos.PessoaNaoEncontrada` |
                | Pessoa sem inscrição confirmada prévia no evento | `409 Eventos.PessoaJaInscrita` |
                | Capacidade (`eventoCapacidadeMaxima` ou, se nula e há local, a capacidade total do local) não atingida | `422 Eventos.CapacidadeEsgotada` |

                **Perfil exigido:** qualquer usuário autenticado.
                """)
            .WithValidation<InscreverParticipanteRequest>()
            .Produces<InscreverParticipanteResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
