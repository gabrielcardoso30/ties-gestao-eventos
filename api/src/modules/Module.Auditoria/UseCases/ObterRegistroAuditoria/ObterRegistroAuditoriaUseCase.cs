using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auditoria.Domain;
using Module.Auditoria.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Auditoria.UseCases.ObterRegistroAuditoria;

internal sealed class ObterRegistroAuditoriaUseCase(AuditoriaDbContext db, ILogger<ObterRegistroAuditoriaUseCase> logger) : IUseCase<ObterRegistroAuditoriaRequest, ObterRegistroAuditoriaResponse>
{
    public async Task<Result<ObterRegistroAuditoriaResponse>> HandleAsync(ObterRegistroAuditoriaRequest request, CancellationToken cancellationToken)
    {
        var registro = await db.RegistrosAuditoria
            .TagWith("Auditoria.ObterRegistroAuditoria")
            .AsNoTracking()
            .Where(r => r.Id == request.RegistroId)
            .Select(r => new ObterRegistroAuditoriaResponse(
                r.Id, r.Modulo, r.EntidadeNome, r.EntidadeId, r.Operacao, r.DadosAnteriores, r.DadosNovos,
                r.UsuarioId, r.UsuarioNome, r.TraceId, r.OcorridoEm, r.RegistradoEm))
            .FirstOrDefaultAsync(cancellationToken);

        if (registro is null)
        {
            logger.LogInformation("Registro de auditoria {AuditRecordId} não encontrado", request.RegistroId);
            return AuditoriaErros.RegistroNaoEncontrado;
        }

        logger.LogInformation("Registro de auditoria {AuditRecordId} carregado para {Module}.{EntityType}", registro.Id, registro.Modulo, registro.EntidadeNome);
        return registro;
    }
}
