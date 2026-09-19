using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Eventos;
using Shared.Contracts.Pessoas;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ValidarCertificado;

/// <summary>
/// Validação pública de certificado pelo código. Consulta o certificado primeiro e só depois enriquece via contratos.
/// A palestra é lida ignorando o filtro de soft delete: certificados permanecem válidos após a exclusão lógica da palestra.
/// </summary>
internal sealed class ValidarCertificadoUseCase(PalestrasDbContext db, IEventosModuleApi eventosApi, IPessoasModuleApi pessoasApi, ILogger<ValidarCertificadoUseCase> logger) : IUseCase<ValidarCertificadoRequest, ValidarCertificadoResponse>
{
    public async Task<Result<ValidarCertificadoResponse>> HandleAsync(ValidarCertificadoRequest request, CancellationToken cancellationToken)
    {
        var codigo = request.CertificadoCodigo.Trim().ToUpperInvariant();

        var certificado = await db.Certificados
            .TagWith("Palestras.ValidarCertificado")
            .AsNoTracking()
            .Where(c => c.CertificadoCodigo == codigo)
            .Select(c => new { c.PalestraId, c.PessoaId, c.CertificadoCodigo, c.CertificadoEmitidoEm, c.CertificadoCargaHorariaMinutos })
            .FirstOrDefaultAsync(cancellationToken);
        if (certificado is null)
        {
            logger.LogInformation("Código de certificado não encontrado durante validação pública");
            return PalestrasErros.CertificadoNaoEncontrado;
        }

        logger.LogDebug("Certificado localizado; carregando palestra {TalkId} inclusive se excluída logicamente", certificado.PalestraId);

        var palestra = await db.Palestras
            .TagWith("Palestras.ValidarCertificado.Palestra")
            .AsNoTracking()
            .IgnoreQueryFilters([ModuleDbContext.SoftDeleteFilterName])
            .Where(p => p.Id == certificado.PalestraId)
            .Select(p => new { p.EventoId, p.PalestraTitulo, p.PalestraInicio })
            .FirstOrDefaultAsync(cancellationToken);
        if (palestra is null)
        {
            logger.LogWarning("Certificado referencia palestra {TalkId} inexistente mesmo ignorando exclusão lógica", certificado.PalestraId);
            return PalestrasErros.CertificadoNaoEncontrado;
        }

        logger.LogDebug("Consultando evento {EventId} para enriquecer a validação do certificado", palestra.EventoId);
        var evento = await eventosApi.ObterEventoResumoAsync(palestra.EventoId, cancellationToken);
        logger.LogDebug("Consultando pessoa {PersonId} para enriquecer a validação do certificado", certificado.PessoaId);
        var pessoa = await pessoasApi.ObterPessoaResumoAsync(certificado.PessoaId, cancellationToken);
        logger.LogInformation("Certificado validado para palestra {TalkId}; referências ausentes: evento={MissingEvent}, pessoa={MissingPerson}", certificado.PalestraId, evento is null, pessoa is null);

        return new ValidarCertificadoResponse(
            certificado.CertificadoCodigo,
            palestra.PalestraTitulo,
            evento?.EventoNome ?? string.Empty,
            pessoa?.PessoaNome ?? string.Empty,
            palestra.PalestraInicio,
            certificado.CertificadoEmitidoEm,
            certificado.CertificadoCargaHorariaMinutos);
    }
}
