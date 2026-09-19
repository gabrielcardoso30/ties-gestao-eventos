using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Pessoas.Domain;
using Module.Pessoas.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.AtualizarPessoa;

internal sealed class AtualizarPessoaUseCase(PessoasDbContext db, ILogger<AtualizarPessoaUseCase> logger) : IUseCase<AtualizarPessoaRequest, AtualizarPessoaResponse>
{
    public async Task<Result<AtualizarPessoaResponse>> HandleAsync(AtualizarPessoaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await db.Pessoas
            .TagWith("Pessoas.AtualizarPessoa.Carregar")
            .FirstOrDefaultAsync(p => p.Id == request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            logger.LogInformation("Pessoa {PessoaId} não encontrada para atualização", request.PessoaId);
            return PessoasErros.PessoaNaoEncontrada;
        }

        logger.LogDebug("Normalizando e verificando unicidade do e-mail da pessoa {PessoaId}", pessoa.Id);
        var email = Pessoa.NormalizarEmail(request.PessoaEmail);
        var emailEmUso = await db.Pessoas
            .TagWith("Pessoas.AtualizarPessoa.VerificarEmail")
            .AnyAsync(p => p.Id != request.PessoaId && p.PessoaEmail == email, cancellationToken);
        if (emailEmUso)
        {
            logger.LogInformation("Atualização da pessoa {PessoaId} rejeitada porque o e-mail normalizado já está em uso", pessoa.Id);
            return PessoasErros.EmailJaCadastrado;
        }

        var documento = Cpf.Normalizar(request.PessoaDocumento);
        logger.LogDebug("Documento informado para pessoa {PessoaId}={HasDocument}", pessoa.Id, documento is not null);
        if (documento is not null)
        {
            var documentoEmUso = await db.Pessoas
                .TagWith("Pessoas.AtualizarPessoa.VerificarDocumento")
                .AnyAsync(p => p.Id != request.PessoaId && p.PessoaDocumento == documento, cancellationToken);
            if (documentoEmUso)
            {
                logger.LogInformation("Atualização da pessoa {PessoaId} rejeitada porque o documento normalizado já está em uso", pessoa.Id);
                return PessoasErros.DocumentoJaCadastrado;
            }
        }

        logger.LogDebug("Chamando agregado Pessoa {PessoaId} para atualizar cadastro e ativo={Active}", pessoa.Id, request.EstaAtivo);
        pessoa.Atualizar(
            request.PessoaNome, request.PessoaEmail, request.PessoaTelefone, request.PessoaDocumento,
            request.PessoaEmpresa, request.PessoaCargo, request.PessoaMiniBio, request.PessoaFotoUrl, request.EstaAtivo);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Pessoa {PessoaId} atualizada; ativo={Active}", pessoa.Id, pessoa.EstaAtivo);
            return Result.Success(new AtualizarPessoaResponse(pessoa.Id, pessoa.PessoaNome, pessoa.PessoaEmail, pessoa.EstaAtivo, pessoa.AlteradoEm));
        }, cancellationToken);
    }
}
