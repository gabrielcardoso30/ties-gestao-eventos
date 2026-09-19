using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.EmitirCertificado;

internal sealed class EmitirCertificadoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id:guid}/certificados", async (Guid id, EmitirCertificadoRequest request, IUseCase<EmitirCertificadoRequest, EmitirCertificadoResponse> useCase, CancellationToken ct) =>
            {
                var resultado = await useCase.HandleAsync(request with { PalestraId = id }, ct);
                return resultado.IsSuccess && !resultado.Value.Criado
                    ? resultado.ToHttpResult()
                    : resultado.ToCreatedResult(r => $"/api/v1/palestras/certificados/{r.CertificadoCodigo}");
            })
            .WithName("EmitirCertificado")
            .WithSummary("Emite o certificado de participação de uma pessoa")
            .WithDescription("""
                Emite o certificado da pessoa na palestra e devolve o **código de validação** (12 caracteres, sem símbolos ambíguos).

                - Exige presença registrada: `422 Palestras.PresencaNaoRegistrada`.
                - Exige palestra encerrada (`palestraFim` já passou): `422 Palestras.PalestraNaoEncerrada`.
                - **Idempotente:** se o certificado já existir, responde `200` com o existente; caso contrário `201` e emite `CertificadoEmitido`.

                A carga horária registrada é a duração da palestra em minutos. Qualquer usuário autenticado pode solicitar.
                """)
            .WithValidation<EmitirCertificadoRequest>()
            .Produces<EmitirCertificadoResponse>(StatusCodes.Status201Created)
            .Produces<EmitirCertificadoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
