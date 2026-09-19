using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.AtualizarSala;

internal sealed class AtualizarSalaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/{id:guid}/salas/{salaId:guid}", async (Guid id, Guid salaId, AtualizarSalaRequest request, IUseCase<AtualizarSalaRequest, AtualizarSalaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { LocalId = id, SalaId = salaId }, ct)).ToHttpResult())
            .WithName("AtualizarSala")
            .WithSummary("Atualiza uma sala")
            .WithDescription("Atualiza nome, capacidade, tipo, recursos e disponibilidade (`estaAtivo=false` torna a sala indisponível para novas palestras). **Perfil exigido:** Administrador ou Organizador.")
            .WithValidation<AtualizarSalaRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AtualizarSalaResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
