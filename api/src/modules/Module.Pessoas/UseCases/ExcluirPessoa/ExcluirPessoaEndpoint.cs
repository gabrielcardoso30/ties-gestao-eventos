using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.ExcluirPessoa;

internal sealed class ExcluirPessoaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/{id:guid}", async (Guid id, IUseCase<ExcluirPessoaRequest, ExcluirPessoaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(new ExcluirPessoaRequest(id), ct)).ToNoContentResult())
            .WithName("ExcluirPessoa")
            .WithSummary("Exclui (logicamente) uma pessoa")
            .WithDescription("""
                Soft delete: o registro permanece no banco com `ExcluidoEm`/`ExcluidoPor` preenchidos e deixa de ser retornado.
                O e-mail e o CPF ficam liberados para um novo cadastro. Publica o evento de integração `PessoaExcluida`.

                **Erros:** `404 Pessoas.PessoaNaoEncontrada`. **Perfil exigido:** Administrador ou Organizador.
                """)
            .RequireAuthorization(Politicas.Gestao)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
