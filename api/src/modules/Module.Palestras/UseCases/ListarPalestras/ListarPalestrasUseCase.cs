using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ListarPalestras;

internal sealed class ListarPalestrasUseCase(PalestrasDbContext db, ILogger<ListarPalestrasUseCase> logger) : IUseCase<ListarPalestrasRequest, PagedResult<ListarPalestrasItemResponse>>
{
    public async Task<Result<PagedResult<ListarPalestrasItemResponse>>> HandleAsync(ListarPalestrasRequest request, CancellationToken cancellationToken)
    {
        var query = db.Palestras.TagWith("Palestras.ListarPalestras").AsNoTracking();
        logger.LogDebug("Montando listagem de palestras: evento={HasEvent}, busca={HasSearch}", request.EventoId.HasValue, !string.IsNullOrWhiteSpace(request.Busca));

        if (request.EventoId.HasValue)
        {
            query = query.Where(p => p.EventoId == request.EventoId.Value);
            logger.LogDebug("Filtro de evento {EventoId} aplicado à listagem de palestras", request.EventoId);
        }

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.PalestraTitulo, busca));
            logger.LogDebug("Filtro textual aplicado à listagem de palestras sem registrar seu conteúdo");
        }

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        logger.LogDebug("Ordenando palestras por {SortField} em direção {SortDirection}", request.OrdenarPor ?? "palestraInicio", request.Direcao);
        var ordenada = request.OrdenarPor?.ToLowerInvariant() switch
        {
            "palestratitulo" => descendente ? query.OrderByDescending(x => x.PalestraTitulo).ThenByDescending(x => x.Id) : query.OrderBy(x => x.PalestraTitulo).ThenBy(x => x.Id),
            "palestrantesquantidade" => descendente ? query.OrderByDescending(x => x.Palestrantes.Count).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Palestrantes.Count).ThenBy(x => x.Id),
            "presencasquantidade" => descendente ? query.OrderByDescending(x => x.Presencas.Count).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Presencas.Count).ThenBy(x => x.Id),
            _ => descendente ? query.OrderByDescending(x => x.PalestraInicio).ThenByDescending(x => x.Id) : query.OrderBy(x => x.PalestraInicio).ThenBy(x => x.Id)
        };
        var pagina = await ordenada
            .Select(p => new ListarPalestrasItemResponse(p.Id, p.EventoId, p.TrilhaId, p.SalaId, p.PalestraTitulo, p.PalestraInicio, p.PalestraFim, p.Palestrantes.Count, p.Presencas.Count))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        logger.LogInformation("Listagem de palestras retornou {ReturnedCount} de {TotalCount} registro(s)", pagina.Itens.Count, pagina.Total);

        return pagina;
    }
}
