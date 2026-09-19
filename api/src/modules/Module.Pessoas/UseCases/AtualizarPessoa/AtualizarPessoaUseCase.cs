using Microsoft.EntityFrameworkCore;
using Module.Pessoas.Domain;
using Module.Pessoas.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.AtualizarPessoa;

internal sealed class AtualizarPessoaUseCase(PessoasDbContext db) : IUseCase<AtualizarPessoaRequest, AtualizarPessoaResponse>
{
    public async Task<Result<AtualizarPessoaResponse>> HandleAsync(AtualizarPessoaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await db.Pessoas
            .TagWith("Pessoas.AtualizarPessoa.Carregar")
            .FirstOrDefaultAsync(p => p.Id == request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            return PessoasErros.PessoaNaoEncontrada;
        }

        var email = Pessoa.NormalizarEmail(request.PessoaEmail);
        var emailEmUso = await db.Pessoas
            .TagWith("Pessoas.AtualizarPessoa.VerificarEmail")
            .AnyAsync(p => p.Id != request.PessoaId && p.PessoaEmail == email, cancellationToken);
        if (emailEmUso)
        {
            return PessoasErros.EmailJaCadastrado;
        }

        var documento = Cpf.Normalizar(request.PessoaDocumento);
        if (documento is not null)
        {
            var documentoEmUso = await db.Pessoas
                .TagWith("Pessoas.AtualizarPessoa.VerificarDocumento")
                .AnyAsync(p => p.Id != request.PessoaId && p.PessoaDocumento == documento, cancellationToken);
            if (documentoEmUso)
            {
                return PessoasErros.DocumentoJaCadastrado;
            }
        }

        pessoa.Atualizar(
            request.PessoaNome, request.PessoaEmail, request.PessoaTelefone, request.PessoaDocumento,
            request.PessoaEmpresa, request.PessoaCargo, request.PessoaMiniBio, request.PessoaFotoUrl, request.EstaAtivo);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AtualizarPessoaResponse(pessoa.Id, pessoa.PessoaNome, pessoa.PessoaEmail, pessoa.EstaAtivo, pessoa.AlteradoEm));
        }, cancellationToken);
    }
}
