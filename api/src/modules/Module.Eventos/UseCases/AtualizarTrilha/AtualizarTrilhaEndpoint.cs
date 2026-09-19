using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;
namespace Module.Eventos.UseCases.AtualizarTrilha;
internal sealed class AtualizarTrilhaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) => group.MapPut("/{id:guid}/trilhas/{trilhaId:guid}", async (Guid id, Guid trilhaId, AtualizarTrilhaRequest request, IUseCase<AtualizarTrilhaRequest, AtualizarTrilhaResponse> useCase, CancellationToken ct) => (await useCase.HandleAsync(request with { EventoId = id, TrilhaId = trilhaId }, ct)).ToHttpResult())
        .WithName("AtualizarTrilha").WithSummary("Atualiza uma trilha do evento").WithDescription("Atualiza identificação e disponibilidade da trilha. **Perfil exigido:** Administrador ou Organizador.").WithValidation<AtualizarTrilhaRequest>().RequireAuthorization(Politicas.Gestao).Produces<AtualizarTrilhaResponse>().ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
}
