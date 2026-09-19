using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;
namespace Module.Eventos.UseCases.ListarTrilhas;
internal sealed class ListarTrilhasEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) => group.MapGet("/{id:guid}/trilhas", async ([AsParameters] ListarTrilhasRequest request, IUseCase<ListarTrilhasRequest, IReadOnlyList<ListarTrilhasItemResponse>> useCase, CancellationToken ct) => (await useCase.HandleAsync(request, ct)).ToHttpResult())
        .WithName("ListarTrilhas").WithSummary("Lista as trilhas de um evento").WithDescription("Retorna as trilhas temáticas ordenadas por nome; use `estaAtivo=true` para seleção em palestras.").WithValidation<ListarTrilhasRequest>().Produces<IReadOnlyList<ListarTrilhasItemResponse>>().ProducesProblem(StatusCodes.Status404NotFound);
}
