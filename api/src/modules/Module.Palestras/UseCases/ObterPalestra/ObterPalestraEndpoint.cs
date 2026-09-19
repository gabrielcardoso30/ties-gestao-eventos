using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ObterPalestra;

internal sealed class ObterPalestraEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}", async (Guid id, IUseCase<ObterPalestraRequest, ObterPalestraResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ObterPalestraRequest(id), ct)).ToHttpResult())
            .WithName("ObterPalestra")
            .WithSummary("Obtém uma palestra com palestrantes e conteúdos")
            .WithDescription("""
                Retorna a palestra com carga horária calculada (`palestraFim - palestraInicio`, em minutos), palestrantes, conteúdos e
                quantidades de presenças e certificados.

                Os campos `eventoNome`, `salaNome` e `pessoaNome` são enriquecidos via contratos dos módulos Eventos, Locais e Pessoas.
                """)
            .Produces<ObterPalestraResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
