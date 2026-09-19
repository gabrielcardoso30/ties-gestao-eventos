using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.AdicionarPalestrante;

internal sealed class AdicionarPalestranteEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id:guid}/palestrantes", async (Guid id, AdicionarPalestranteRequest request, IUseCase<AdicionarPalestranteRequest, AdicionarPalestranteResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { PalestraId = id }, ct)).ToCreatedResult(r => $"/api/v1/palestras/{r.PalestraId}"))
            .WithName("AdicionarPalestrante")
            .WithSummary("Vincula uma pessoa como palestrante")
            .WithDescription("""
                Adiciona uma pessoa (módulo Pessoas) à palestra com o papel `Principal`, `Coautor` ou `Mediador`.

                - A pessoa precisa existir (`422 Palestras.PessoaNaoEncontrada`).
                - Uma pessoa só é vinculada uma vez por palestra (`409 Palestras.PalestranteJaVinculado`).

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AdicionarPalestranteRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AdicionarPalestranteResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
