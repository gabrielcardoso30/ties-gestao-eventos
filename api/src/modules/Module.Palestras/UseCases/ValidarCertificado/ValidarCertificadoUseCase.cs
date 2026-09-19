using Microsoft.EntityFrameworkCore;
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
internal sealed class ValidarCertificadoUseCase(PalestrasDbContext db, IEventosModuleApi eventosApi, IPessoasModuleApi pessoasApi) : IUseCase<ValidarCertificadoRequest, ValidarCertificadoResponse>
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
            return PalestrasErros.CertificadoNaoEncontrado;
        }

        var palestra = await db.Palestras
            .TagWith("Palestras.ValidarCertificado.Palestra")
            .AsNoTracking()
            .IgnoreQueryFilters([ModuleDbContext.SoftDeleteFilterName])
            .Where(p => p.Id == certificado.PalestraId)
            .Select(p => new { p.EventoId, p.PalestraTitulo, p.PalestraInicio })
            .FirstOrDefaultAsync(cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.CertificadoNaoEncontrado;
        }

        var evento = await eventosApi.ObterEventoResumoAsync(palestra.EventoId, cancellationToken);
        var pessoa = await pessoasApi.ObterPessoaResumoAsync(certificado.PessoaId, cancellationToken);

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
