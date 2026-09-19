using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Common;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.ListarLocais;

internal sealed class ListarLocaisEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("", async ([AsParameters] ListarLocaisRequest request, IUseCase<ListarLocaisRequest, PagedResult<ListarLocaisItemResponse>> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToHttpResult())
            .WithName("ListarLocais")
            .WithSummary("Lista locais (paginado)")
            .WithDescription("""
                Lista locais ordenados por nome, com filtros opcionais:

                | Parâmetro | Descrição |
                |-----------|-----------|
                | `busca` | Texto contido no nome ou na cidade (case-insensitive) |
                | `enderecoUf` | UF com 2 letras |
                | `estaAtivo` | `true`/`false` |
                | `pagina`, `tamanhoPagina` | Paginação (máx. 100 por página) |
                """)
            .WithValidation<ListarLocaisRequest>()
            .Produces<PagedResult<ListarLocaisItemResponse>>();
}
