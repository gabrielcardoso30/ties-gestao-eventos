using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.AtualizarPalestra;

internal sealed class AtualizarPalestraEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/{id:guid}", async (Guid id, AtualizarPalestraRequest request, IUseCase<AtualizarPalestraRequest, AtualizarPalestraResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { PalestraId = id }, ct)).ToHttpResult())
            .WithName("AtualizarPalestra")
            .WithSummary("Atualiza os dados de uma palestra")
            .WithDescription("""
                Atualiza título, descrição, sala e horário. O evento da palestra não muda; palestrantes e conteúdos são gerenciados em `/palestrantes` e `/conteudos`.

                As mesmas regras da criação são reaplicadas: evento ainda aceita palestras (`422 Palestras.EventoNaoAceitaPalestras`),
                período dentro do evento (`422 Palestras.PeriodoForaDoEvento`), sala do local do evento (`422 Palestras.SalaNaoPertenceAoLocal`)
                e sem sobreposição com **outras** palestras na sala (`409 Palestras.SalaOcupada`).

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AtualizarPalestraRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AtualizarPalestraResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
}
