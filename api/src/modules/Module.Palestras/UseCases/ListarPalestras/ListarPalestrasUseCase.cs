using Microsoft.EntityFrameworkCore;
using Module.Palestras.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ListarPalestras;

internal sealed class ListarPalestrasUseCase(PalestrasDbContext db) : IUseCase<ListarPalestrasRequest, PagedResult<ListarPalestrasItemResponse>>
{
    public async Task<Result<PagedResult<ListarPalestrasItemResponse>>> HandleAsync(ListarPalestrasRequest request, CancellationToken cancellationToken)
    {
        var query = db.Palestras.TagWith("Palestras.ListarPalestras").AsNoTracking();

        if (request.EventoId.HasValue)
        {
            query = query.Where(p => p.EventoId == request.EventoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.PalestraTitulo, busca));
        }

        var pagina = await query
            .OrderBy(p => p.PalestraInicio).ThenBy(p => p.PalestraTitulo)
            .Select(p => new ListarPalestrasItemResponse(
                p.Id, p.EventoId, p.SalaId, p.PalestraTitulo, p.PalestraInicio, p.PalestraFim, p.Palestrantes.Count, p.Presencas.Count))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        return pagina;
    }
}
