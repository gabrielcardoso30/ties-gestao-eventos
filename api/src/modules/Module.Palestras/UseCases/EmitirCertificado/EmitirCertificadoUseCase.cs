using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Palestras;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.EmitirCertificado;

/// <summary>Emite (ou devolve o já emitido) o certificado de uma pessoa com presença na palestra encerrada. Emite <see cref="CertificadoEmitido"/> quando cria.</summary>
internal sealed class EmitirCertificadoUseCase(PalestrasDbContext db, IPessoasModuleApi pessoasApi, TimeProvider timeProvider, ILogger<EmitirCertificadoUseCase> logger) : IUseCase<EmitirCertificadoRequest, EmitirCertificadoResponse>
{
    public async Task<Result<EmitirCertificadoResponse>> HandleAsync(EmitirCertificadoRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.EmitirCertificado.Carregar")
            .Include(p => p.Presencas.Where(x => x.PessoaId == request.PessoaId))
            .Include(p => p.Certificados.Where(x => x.PessoaId == request.PessoaId))
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Emissão de certificado rejeitada: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId}.EmitirCertificado para pessoa {PessoaId}; presenças carregadas={AttendanceCount}, certificados existentes={CertificateCount}",
            palestra.Id, request.PessoaId, palestra.Presencas.Count, palestra.Certificados.Count);
        var resultado = palestra.EmitirCertificado(request.PessoaId, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            logger.LogInformation("Certificado da pessoa {PessoaId} para palestra {TalkId} rejeitado pela regra {ErrorCode}", request.PessoaId, palestra.Id, resultado.Error.Code);
            return resultado.Error;
        }

        var (certificado, criado) = resultado.Value;
        logger.LogInformation("Certificado {CertificateId} da pessoa {PessoaId} na palestra {TalkId}: novo={CertificateCreated}", certificado.Id, certificado.PessoaId, palestra.Id, criado);
        if (criado)
        {
            var gravacao = await db.ExecuteInTransactionAsync(async ct =>
            {
                await db.SaveChangesAsync(ct);
                return Result.Success();
            }, cancellationToken);
            if (gravacao.IsFailure)
            {
                logger.LogWarning("Persistência do certificado {CertificateId} falhou pela regra {ErrorCode}", certificado.Id, gravacao.Error.Code);
                return gravacao.Error;
            }
            logger.LogInformation("Novo certificado {CertificateId} persistido", certificado.Id);
        }
        else
        {
            logger.LogDebug("Certificado {CertificateId} já existia; persistência não foi necessária", certificado.Id);
        }

        logger.LogDebug("Consultando módulo Pessoas para enriquecer certificado {CertificateId}", certificado.Id);
        var pessoa = await pessoasApi.ObterPessoaResumoAsync(request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            logger.LogWarning("Pessoa {PessoaId} do certificado {CertificateId} não foi encontrada durante o enriquecimento", request.PessoaId, certificado.Id);
        }
        else
        {
            logger.LogDebug("Pessoa {PessoaId} localizada para enriquecer certificado {CertificateId}", pessoa.Id, certificado.Id);
        }

        return new EmitirCertificadoResponse(
            certificado.Id,
            certificado.CertificadoCodigo,
            palestra.Id,
            palestra.PalestraTitulo,
            certificado.PessoaId,
            pessoa?.PessoaNome ?? string.Empty,
            certificado.CertificadoEmitidoEm,
            certificado.CertificadoCargaHorariaMinutos)
        { Criado = criado };
    }
}
