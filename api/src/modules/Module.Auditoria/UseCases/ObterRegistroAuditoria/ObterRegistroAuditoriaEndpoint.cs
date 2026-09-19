using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Auditoria.UseCases.ObterRegistroAuditoria;

internal sealed class ObterRegistroAuditoriaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/registros/{id:guid}", async (Guid id, IUseCase<ObterRegistroAuditoriaRequest, ObterRegistroAuditoriaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ObterRegistroAuditoriaRequest(id), ct)).ToHttpResult())
            .WithName("ObterRegistroAuditoria")
            .WithSummary("Obtém o detalhe de um registro de auditoria")
            .WithDescription("""
                Retorna o registro completo, incluindo `dadosAnteriores` e `dadosNovos` como **strings JSON**:

                - `Inclusao`: `dadosAnteriores` nulo; `dadosNovos` com todas as propriedades da entidade.
                - `Alteracao`/`Exclusao`: apenas as propriedades modificadas, antes e depois.

                `traceId` permite correlacionar com logs e traces da requisição original. **Erros:** `404 Auditoria.RegistroNaoEncontrado`. **Perfil exigido:** Administrador.
                """)
            .RequireAuthorization(Politicas.Administracao)
            .Produces<ObterRegistroAuditoriaResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
