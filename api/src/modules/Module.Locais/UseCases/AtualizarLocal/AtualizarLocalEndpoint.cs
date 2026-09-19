using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.AtualizarLocal;

internal sealed class AtualizarLocalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/{id:guid}", async (Guid id, AtualizarLocalRequest request, IUseCase<AtualizarLocalRequest, AtualizarLocalResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { LocalId = id }, ct)).ToHttpResult())
            .WithName("AtualizarLocal")
            .WithSummary("Atualiza os dados de um local")
            .WithDescription("Atualiza nome, descrição e endereço. Salas são gerenciadas em `/salas`. **Perfil exigido:** Administrador ou Organizador.")
            .WithValidation<AtualizarLocalRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AtualizarLocalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
