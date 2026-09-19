using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Contracts.Identidade;
using Shared.Http.Endpoints;
using Shared.Http.Results;
using Shared.Http.Validation;

namespace Module.Pessoas.UseCases.CriarPessoa;

internal sealed class CriarPessoaEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("", async (CriarPessoaRequest request, IUseCase<CriarPessoaRequest, CriarPessoaResponse> useCase, CancellationToken ct) =>
                (await useCase.HandleAsync(request, ct)).ToCreatedResult(r => $"/api/v1/pessoas/{r.Id}"))
            .WithName("CriarPessoa")
            .WithSummary("Cadastra uma pessoa")
            .WithDescription("""
                Cadastra uma pessoa (palestrante ou participante; o papel nasce do relacionamento com eventos e palestras).

                | Campo | Regra |
                |-------|-------|
                | `pessoaNome` | Obrigatório, até 150 caracteres |
                | `pessoaEmail` | Obrigatório, e-mail válido; armazenado em minúsculas; único entre pessoas ativas |
                | `pessoaDocumento` | Opcional; CPF com ou sem máscara, validado pelos dígitos verificadores; armazenado só com dígitos; único entre pessoas ativas |
                | `pessoaTelefone`, `pessoaEmpresa`, `pessoaCargo` | Opcionais (20/150/100 caracteres) |
                | `pessoaMiniBio` | Opcional, até 2000 caracteres |
                | `pessoaFotoUrl` | Opcional, URL absoluta http(s), até 500 caracteres |

                **Erros:** `400 Validacao` · `409 Pessoas.EmailJaCadastrado` · `409 Pessoas.DocumentoJaCadastrado`.

                Publica o evento de integração `PessoaCriada`. **Perfil exigido:** Administrador ou Organizador.
                """)
            .WithValidation<CriarPessoaRequest>()
            .RequireAuthorization(Politicas.Gestao)
            .Produces<CriarPessoaResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
