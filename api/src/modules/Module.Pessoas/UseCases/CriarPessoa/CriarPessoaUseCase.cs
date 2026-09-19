using Microsoft.EntityFrameworkCore;
using Module.Pessoas.Domain;
using Module.Pessoas.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.CriarPessoa;

internal sealed class CriarPessoaUseCase(PessoasDbContext db) : IUseCase<CriarPessoaRequest, CriarPessoaResponse>
{
    public async Task<Result<CriarPessoaResponse>> HandleAsync(CriarPessoaRequest request, CancellationToken cancellationToken)
    {
        var email = Pessoa.NormalizarEmail(request.PessoaEmail);
        var emailEmUso = await db.Pessoas
            .TagWith("Pessoas.CriarPessoa.VerificarEmail")
            .AnyAsync(p => p.PessoaEmail == email, cancellationToken);
        if (emailEmUso)
        {
            return PessoasErros.EmailJaCadastrado;
        }

        var documento = Cpf.Normalizar(request.PessoaDocumento);
        if (documento is not null)
        {
            var documentoEmUso = await db.Pessoas
                .TagWith("Pessoas.CriarPessoa.VerificarDocumento")
                .AnyAsync(p => p.PessoaDocumento == documento, cancellationToken);
            if (documentoEmUso)
            {
                return PessoasErros.DocumentoJaCadastrado;
            }
        }

        var pessoa = Pessoa.Criar(
            request.PessoaNome, request.PessoaEmail, request.PessoaTelefone, request.PessoaDocumento,
            request.PessoaEmpresa, request.PessoaCargo, request.PessoaMiniBio, request.PessoaFotoUrl);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Pessoas.Add(pessoa);
            await db.SaveChangesAsync(ct);
            return Result.Success(new CriarPessoaResponse(pessoa.Id, pessoa.PessoaNome, pessoa.PessoaEmail));
        }, cancellationToken);
    }
}
