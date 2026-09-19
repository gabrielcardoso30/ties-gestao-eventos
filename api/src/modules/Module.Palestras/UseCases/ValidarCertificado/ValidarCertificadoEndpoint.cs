using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ValidarCertificado;

internal sealed class ValidarCertificadoEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/certificados/{codigo}", async (string codigo, IUseCase<ValidarCertificadoRequest, ValidarCertificadoResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ValidarCertificadoRequest(codigo), ct)).ToHttpResult())
            .WithName("ValidarCertificado")
            .WithSummary("Valida publicamente um certificado pelo código")
            .WithDescription("""
                Endpoint **público** (sem autenticação) para conferência de autenticidade de certificados.

                Informe o código de 12 caracteres impresso no certificado. Retorna título da palestra, evento, nome do participante,
                data da palestra, data de emissão e carga horária. Código desconhecido: `404 Palestras.CertificadoNaoEncontrado`.
                """)
            .AllowAnonymous()
            .Produces<ValidarCertificadoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
