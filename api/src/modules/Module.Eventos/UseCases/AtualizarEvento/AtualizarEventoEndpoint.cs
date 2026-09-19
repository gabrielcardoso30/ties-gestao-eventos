using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.AtualizarEvento;

internal sealed class AtualizarEventoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/{id:guid}", async (Guid id, AtualizarEventoRequest request, IUseCase<AtualizarEventoRequest, AtualizarEventoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { EventoId = id }, ct)).ToHttpResult())
            .WithName("AtualizarEvento")
            .WithSummary("Atualiza os dados de um evento")
            .WithDescription("""
                Atualiza nome, descrição, período, formato, local/link e capacidade. A situação é alterada apenas em `PATCH /{id}/situacao`.

                - Permitido somente em `Rascunho` ou `Publicado` (`422 Eventos.EventoNaoPodeSerAlterado`).
                - Mesmas regras de consistência de formato da criação; `localId` precisa existir (`422 Eventos.LocalNaoEncontrado`).

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AtualizarEventoRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AtualizarEventoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
