using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ExcluirEvento;

internal sealed class ExcluirEventoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}", async (Guid id, IUseCase<ExcluirEventoRequest, ExcluirEventoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ExcluirEventoRequest(id), ct)).ToNoContentResult())
            .WithName("ExcluirEvento")
            .WithSummary("Exclui (logicamente) um evento")
            .WithDescription("""
                Soft delete do evento e de suas inscrições: registros permanecem no banco com `ExcluidoEm`/`ExcluidoPor` preenchidos.

                Permitido somente em `Rascunho` ou `Cancelado` (`422 Eventos.EventoNaoPodeSerExcluido`). **Perfil exigido:** Administrador ou Organizador.
                """)
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
