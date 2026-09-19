using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.AdicionarSala;

internal sealed class AdicionarSalaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id:guid}/salas", async (Guid id, AdicionarSalaRequest request, IUseCase<AdicionarSalaRequest, AdicionarSalaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { LocalId = id }, ct)).ToCreatedResult(r => $"/api/v1/locais/{r.LocalId}/salas/{r.Id}"))
            .WithName("AdicionarSala")
            .WithSummary("Adiciona uma sala (ambiente) a um local")
            .WithDescription("""
                Tipos de sala: `AmbienteUnico`, `Auditorio`, `SalaAula`, `Laboratorio`, `AreaRecreacao`, `Coworking`, `Outro`.

                O nome da sala é único dentro do local (`409 Locais.SalaNomeDuplicado`). **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AdicionarSalaRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AdicionarSalaResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
