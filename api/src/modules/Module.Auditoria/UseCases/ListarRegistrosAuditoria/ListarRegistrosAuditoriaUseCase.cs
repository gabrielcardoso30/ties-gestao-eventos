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

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        var ordenada = request.OrdenarPor?.ToLowerInvariant() switch
        {
            "modulo" => descendente ? query.OrderByDescending(x => x.Modulo).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Modulo).ThenBy(x => x.Id),
            "entidadenome" => descendente ? query.OrderByDescending(x => x.EntidadeNome).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EntidadeNome).ThenBy(x => x.Id),
            "operacao" => descendente ? query.OrderByDescending(x => x.Operacao).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Operacao).ThenBy(x => x.Id),
            "usuarionome" => descendente ? query.OrderByDescending(x => x.UsuarioNome).ThenByDescending(x => x.Id) : query.OrderBy(x => x.UsuarioNome).ThenBy(x => x.Id),
            _ => descendente ? query.OrderByDescending(x => x.OcorridoEm).ThenByDescending(x => x.Id) : query.OrderBy(x => x.OcorridoEm).ThenBy(x => x.Id)
        };
        var pagina = await ordenada
            .Select(r => new ListarRegistrosAuditoriaItemResponse(r.Id, r.Modulo, r.EntidadeNome, r.EntidadeId, r.Operacao, r.UsuarioNome, r.TraceId, r.OcorridoEm))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        return pagina;
    }
}
