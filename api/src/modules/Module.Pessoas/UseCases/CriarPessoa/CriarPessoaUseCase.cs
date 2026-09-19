using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Pessoas.Domain;
using Module.Pessoas.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.CriarPessoa;

internal sealed class CriarPessoaUseCase(PessoasDbContext db, ILogger<CriarPessoaUseCase> logger) : IUseCase<CriarPessoaRequest, CriarPessoaResponse>
{
    public async Task<Result<CriarPessoaResponse>> HandleAsync(CriarPessoaRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Normalizando e verificando unicidade do e-mail da nova pessoa");
        var email = Pessoa.NormalizarEmail(request.PessoaEmail);
        var emailEmUso = await db.Pessoas
            .TagWith("Pessoas.CriarPessoa.VerificarEmail")
            .AnyAsync(p => p.PessoaEmail == email, cancellationToken);
        if (emailEmUso)
        {
            logger.LogInformation("Criação de pessoa rejeitada porque o e-mail normalizado já está cadastrado");
            return PessoasErros.EmailJaCadastrado;
        }

        var documento = Cpf.Normalizar(request.PessoaDocumento);
        logger.LogDebug("Documento informado para nova pessoa={HasDocument}", documento is not null);
        if (documento is not null)
        {
            var documentoEmUso = await db.Pessoas
                .TagWith("Pessoas.CriarPessoa.VerificarDocumento")
                .AnyAsync(p => p.PessoaDocumento == documento, cancellationToken);
            if (documentoEmUso)
            {
                logger.LogInformation("Criação de pessoa rejeitada porque o documento normalizado já está cadastrado");
                return PessoasErros.DocumentoJaCadastrado;
            }
        }

        logger.LogDebug("Chamando fábrica de domínio Pessoa.Criar");
        var pessoa = Pessoa.Criar(
            request.PessoaNome, request.PessoaEmail, request.PessoaTelefone, request.PessoaDocumento,
            request.PessoaEmpresa, request.PessoaCargo, request.PessoaMiniBio, request.PessoaFotoUrl);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Pessoas.Add(pessoa);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Pessoa {PessoaId} criada", pessoa.Id);
            return Result.Success(new CriarPessoaResponse(pessoa.Id, pessoa.PessoaNome, pessoa.PessoaEmail));
        }, cancellationToken);
    }
}
