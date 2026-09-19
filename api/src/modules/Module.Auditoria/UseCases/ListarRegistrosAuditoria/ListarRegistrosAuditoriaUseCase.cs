using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auditoria.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Auditoria.UseCases.ListarRegistrosAuditoria;

internal sealed class ListarRegistrosAuditoriaUseCase(AuditoriaDbContext db, ILogger<ListarRegistrosAuditoriaUseCase> logger) : IUseCase<ListarRegistrosAuditoriaRequest, PagedResult<ListarRegistrosAuditoriaItemResponse>>
{
    public async Task<Result<PagedResult<ListarRegistrosAuditoriaItemResponse>>> HandleAsync(ListarRegistrosAuditoriaRequest request, CancellationToken cancellationToken)
    {
        var query = db.RegistrosAuditoria.TagWith("Auditoria.ListarRegistrosAuditoria").AsNoTracking();
        logger.LogDebug("Montando consulta de auditoria: módulo={HasModule}, entidade={HasEntity}, entidadeId={HasEntityId}, usuário={HasUser}, operação={HasOperation}, início={HasStart}, fim={HasEnd}",
            !string.IsNullOrWhiteSpace(request.Modulo), !string.IsNullOrWhiteSpace(request.EntidadeNome), !string.IsNullOrWhiteSpace(request.EntidadeId), request.UsuarioId.HasValue,
            !string.IsNullOrWhiteSpace(request.Operacao), request.OcorridoDe.HasValue, request.OcorridoAte.HasValue);

        if (!string.IsNullOrWhiteSpace(request.Modulo))
        {
            var modulo = request.Modulo.Trim();
            query = query.Where(r => r.Modulo == modulo);
            logger.LogDebug("Filtro de módulo aplicado à consulta de auditoria");
        }

        if (!string.IsNullOrWhiteSpace(request.EntidadeNome))
        {
            var entidadeNome = request.EntidadeNome.Trim();
            query = query.Where(r => r.EntidadeNome == entidadeNome);
            logger.LogDebug("Filtro de tipo de entidade aplicado à consulta de auditoria");
        }

        if (!string.IsNullOrWhiteSpace(request.EntidadeId))
        {
            var entidadeId = request.EntidadeId.Trim();
            query = query.Where(r => r.EntidadeId == entidadeId);
            logger.LogDebug("Filtro de identificador de entidade aplicado à consulta de auditoria");
        }

        if (request.UsuarioId.HasValue)
        {
            query = query.Where(r => r.UsuarioId == request.UsuarioId.Value);
            logger.LogDebug("Filtro de usuário {UsuarioId} aplicado à consulta de auditoria", request.UsuarioId);
        }

        if (!string.IsNullOrWhiteSpace(request.Operacao))
        {
            var operacao = request.Operacao.Trim();
            query = query.Where(r => EF.Functions.ILike(r.Operacao, operacao));
            logger.LogDebug("Filtro de operação aplicado à consulta de auditoria");
        }

        if (request.OcorridoDe.HasValue)
        {
            query = query.Where(r => r.OcorridoEm >= request.OcorridoDe.Value);
            logger.LogDebug("Limite inicial aplicado à consulta de auditoria");
        }

        if (request.OcorridoAte.HasValue)
        {
            query = query.Where(r => r.OcorridoEm <= request.OcorridoAte.Value);
            logger.LogDebug("Limite final aplicado à consulta de auditoria");
        }

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        logger.LogDebug("Ordenando auditoria por {SortField} em direção {SortDirection}", request.OrdenarPor ?? "ocorridoEm", request.Direcao);
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

        logger.LogInformation("Consulta de auditoria retornou {ReturnedCount} de {TotalCount} registro(s)", pagina.Itens.Count, pagina.Total);

        return pagina;
    }
}
