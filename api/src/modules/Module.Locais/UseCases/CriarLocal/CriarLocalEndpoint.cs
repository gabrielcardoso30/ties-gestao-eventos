using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.CriarLocal;

internal sealed class CriarLocalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("", async (CriarLocalRequest request, IUseCase<CriarLocalRequest, CriarLocalResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToCreatedResult(r => $"/api/v1/locais/{r.Id}"))
            .WithName("CriarLocal")
            .WithSummary("Cria um local")
            .WithDescription("""
                Cria um local para eventos presenciais.

                - Se `capacidadeAmbienteUnico` for informado, o local nasce com a sala **"Ambiente único"** (regra: local de um ambiente = uma sala).
                - Caso contrário, adicione salas em `POST /api/v1/locais/{id}/salas`.
                - O nome do local é único entre os locais ativos (`409 Locais.LocalNomeDuplicado`).

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<CriarLocalRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<CriarLocalResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
