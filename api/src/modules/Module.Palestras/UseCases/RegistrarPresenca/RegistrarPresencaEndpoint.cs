using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.RegistrarPresenca;

internal sealed class RegistrarPresencaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id:guid}/presencas", async (Guid id, RegistrarPresencaRequest request, IUseCase<RegistrarPresencaRequest, RegistrarPresencaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { PalestraId = id }, ct)).ToCreatedResult(r => $"/api/v1/palestras/{r.PalestraId}/presencas"))
            .WithName("RegistrarPresenca")
            .WithSummary("Registra a presença de um participante na palestra")
            .WithDescription("""
                Registra a presença de uma pessoa na palestra (check-in).

                - A pessoa precisa ter **inscrição confirmada** no evento da palestra (módulo Eventos): `422 Palestras.ParticipanteNaoInscrito`.
                - A presença é única por pessoa e palestra: `409 Palestras.PresencaJaRegistrada`.

                Emite o evento de integração `PresencaRegistrada`. **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<RegistrarPresencaRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<RegistrarPresencaResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
