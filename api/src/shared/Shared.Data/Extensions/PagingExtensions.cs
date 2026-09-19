using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Common;

namespace Shared.Data.Extensions;

public static class PagingExtensions
{
    /// <summary>Pagina uma consulta já projetada (somente as colunas necessárias) e ordenada.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PagedRequest paging, CancellationToken cancellationToken)
    {
        var pagina = paging.PaginaNormalizada;
        var tamanho = paging.TamanhoNormalizado;
        var total = await query.LongCountAsync(cancellationToken);
        var itens = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(cancellationToken);
        return new PagedResult<T>(itens, pagina, tamanho, total);
    }
}
