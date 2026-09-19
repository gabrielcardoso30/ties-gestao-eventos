using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;
namespace Module.Eventos.UseCases.AdicionarTrilha;
internal sealed class AdicionarTrilhaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) => group.MapPost("/{id:guid}/trilhas", async (Guid id, AdicionarTrilhaRequest request, IUseCase<AdicionarTrilhaRequest, AdicionarTrilhaResponse> useCase, CancellationToken ct) => (await useCase.HandleAsync(request with { EventoId = id }, ct)).ToCreatedResult(r => $"/api/v1/eventos/{r.EventoId}/trilhas/{r.Id}"))
        .WithName("AdicionarTrilha").WithSummary("Adiciona uma trilha temática ao evento").WithDescription("O nome da trilha é único no evento. **Perfil exigido:** Administrador ou Organizador.").WithValidation<AdicionarTrilhaRequest>().RequireAuthorization(Politicas.Gestao).Produces<AdicionarTrilhaResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
}
