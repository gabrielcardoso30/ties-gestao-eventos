using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.CriarEvento;

internal sealed class CriarEventoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("", async (CriarEventoRequest request, IUseCase<CriarEventoRequest, CriarEventoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToCreatedResult(r => $"/api/v1/eventos/{r.Id}"))
            .WithName("CriarEvento")
            .WithSummary("Cria um evento (em rascunho)")
            .WithDescription("""
                Cria um evento na situação **Rascunho**. Publique-o depois em `PATCH /api/v1/eventos/{id}/situacao`.

                | Formato | `localId` | `eventoLinkRemoto` |
                |---------|-----------|--------------------|
                | `Presencial` | obrigatório | — |
                | `Remoto` | não permitido | obrigatório |
                | `Hibrido` | obrigatório | obrigatório |

                - `eventoDataFim` deve ser posterior a `eventoDataInicio`.
                - `localId`, quando informado, precisa existir no módulo Locais (`422 Eventos.LocalNaoEncontrado`).
                - `eventoCapacidadeMaxima` é opcional; se omitida em evento com local, vale a capacidade total do local nas inscrições.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<CriarEventoRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<CriarEventoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
