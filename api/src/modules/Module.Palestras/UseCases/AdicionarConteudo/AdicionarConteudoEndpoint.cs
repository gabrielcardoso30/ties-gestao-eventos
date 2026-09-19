using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.AdicionarConteudo;

internal sealed class AdicionarConteudoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id:guid}/conteudos", async (Guid id, AdicionarConteudoRequest request, IUseCase<AdicionarConteudoRequest, AdicionarConteudoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { PalestraId = id }, ct)).ToCreatedResult(r => $"/api/v1/palestras/{r.PalestraId}"))
            .WithName("AdicionarConteudo")
            .WithSummary("Adiciona um conteúdo (material) à palestra")
            .WithDescription("""
                Registra um material da palestra apontando para uma URL absoluta (armazenamento externo).

                Tipos de conteúdo: `Slides`, `Pdf`, `Arquivo`, `Link`, `Video`, `Imagem`.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AdicionarConteudoRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AdicionarConteudoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
