using Microsoft.EntityFrameworkCore;
using Module.Auditoria.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Auditoria.UseCases.ListarRegistrosAuditoria;

internal sealed class ListarRegistrosAuditoriaUseCase(AuditoriaDbContext db) : IUseCase<ListarRegistrosAuditoriaRequest, PagedResult<ListarRegistrosAuditoriaItemResponse>>
{
    public async Task<Result<PagedResult<ListarRegistrosAuditoriaItemResponse>>> HandleAsync(ListarRegistrosAuditoriaRequest request, CancellationToken cancellationToken)
    {
        var query = db.RegistrosAuditoria.TagWith("Auditoria.ListarRegistrosAuditoria").AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Modulo))
        {
            var modulo = request.Modulo.Trim();
            query = query.Where(r => r.Modulo == modulo);
        }

        if (!string.IsNullOrWhiteSpace(request.EntidadeNome))
        {
            var entidadeNome = request.EntidadeNome.Trim();
            query = query.Where(r => r.EntidadeNome == entidadeNome);
        }

        if (!string.IsNullOrWhiteSpace(request.EntidadeId))
        {
            var entidadeId = request.EntidadeId.Trim();
            query = query.Where(r => r.EntidadeId == entidadeId);
        }

        if (request.UsuarioId.HasValue)
        {
            query = query.Where(r => r.UsuarioId == request.UsuarioId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Operacao))
        {
            var operacao = request.Operacao.Trim();
            query = query.Where(r => EF.Functions.ILike(r.Operacao, operacao));
        }

        if (request.OcorridoDe.HasValue)
        {
            query = query.Where(r => r.OcorridoEm >= request.OcorridoDe.Value);
        }

        if (request.OcorridoAte.HasValue)
        {
            query = query.Where(r => r.OcorridoEm <= request.OcorridoAte.Value);
        }

        var pagina = await query
            .OrderByDescending(r => r.OcorridoEm)
            .ThenByDescending(r => r.Id)
            .Select(r => new ListarRegistrosAuditoriaItemResponse(r.Id, r.Modulo, r.EntidadeNome, r.EntidadeId, r.Operacao, r.UsuarioNome, r.TraceId, r.OcorridoEm))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        return pagina;
    }
}
