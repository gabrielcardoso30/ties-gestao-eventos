using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.AtualizarPessoa;

internal sealed class AtualizarPessoaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/{id:guid}", async (Guid id, AtualizarPessoaRequest request, IUseCase<AtualizarPessoaRequest, AtualizarPessoaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request with { PessoaId = id }, ct)).ToHttpResult())
            .WithName("AtualizarPessoa")
            .WithSummary("Atualiza os dados de uma pessoa")
            .WithDescription("""
                Substitui todos os dados da pessoa (PUT), inclusive `estaAtivo` (pessoas inativas não são retornadas via `IPessoasModuleApi`).

                As mesmas regras do cadastro se aplicam: e-mail normalizado em minúsculas e CPF só com dígitos, ambos únicos entre pessoas ativas.

                **Erros:** `400 Validacao` · `404 Pessoas.PessoaNaoEncontrada` · `409 Pessoas.EmailJaCadastrado` · `409 Pessoas.DocumentoJaCadastrado`.

                **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<AtualizarPessoaRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<AtualizarPessoaResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
