using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.ObterPessoa;

internal sealed class ObterPessoaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id:guid}", async (Guid id, IUseCase<ObterPessoaRequest, ObterPessoaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ObterPessoaRequest(id), ct)).ToHttpResult())
            .WithName("ObterPessoa")
            .WithSummary("Obtém uma pessoa")
            .WithDescription("Retorna os dados cadastrais completos da pessoa (CPF sem máscara). Pessoas excluídas retornam `404 Pessoas.PessoaNaoEncontrada`. **Perfil exigido:** qualquer usuário autenticado.")
            .Produces<ObterPessoaResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
