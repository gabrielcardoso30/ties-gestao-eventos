using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ObterEvento;

internal sealed class ObterEventoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}", async (Guid id, IUseCase<ObterEventoRequest, ObterEventoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ObterEventoRequest(id), ct)).ToHttpResult())
            .WithName("ObterEvento")
            .WithSummary("Obtém um evento")
            .WithDescription("""
                Retorna os dados do evento, a quantidade de inscrições confirmadas e, quando há local, o `localNome` (obtido do módulo Locais).

                **Perfil exigido:** qualquer usuário autenticado.
                """)
            .Produces<ObterEventoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
