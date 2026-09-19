using Microsoft.EntityFrameworkCore;
using Module.Auditoria.Domain;
using Module.Auditoria.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Auditoria.UseCases.ObterRegistroAuditoria;

internal sealed class ObterRegistroAuditoriaUseCase(AuditoriaDbContext db) : IUseCase<ObterRegistroAuditoriaRequest, ObterRegistroAuditoriaResponse>
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

        return registro is null ? AuditoriaErros.RegistroNaoEncontrado : registro;
    }
}
