using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.ListarPalestras;

internal sealed class ListarPalestrasEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("", async ([AsParameters] ListarPalestrasRequest request, IUseCase<ListarPalestrasRequest, PagedResult<ListarPalestrasItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarPalestras")
            .WithSummary("Lista palestras (paginado)")
            .WithDescription("""
                Lista palestras ordenadas por `palestraInicio`, com filtros opcionais:

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `eventoId` | Somente palestras do evento |
                | `busca` | Texto contido no título (case-insensitive) |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |
                """)
            .WithValidation<ListarPalestrasRequest>()
            .Produces<PagedResult<ListarPalestrasItemResponse>>();
}
