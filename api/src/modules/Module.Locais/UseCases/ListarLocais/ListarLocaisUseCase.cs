using Microsoft.EntityFrameworkCore;
using Module.Locais.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ListarLocais;

internal sealed class ListarLocaisUseCase(LocaisDbContext db) : IUseCase<ListarLocaisRequest, PagedResult<ListarLocaisItemResponse>>
{
    public async Task<Result<PagedResult<ListarLocaisItemResponse>>> HandleAsync(ListarLocaisRequest request, CancellationToken cancellationToken)
    {
        var query = db.Locais.TagWith("Locais.ListarLocais").AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.LocalNome, busca) || EF.Functions.ILike(l.EnderecoCidade, busca));
        }

        if (!string.IsNullOrWhiteSpace(request.EnderecoUf))
        {
            var uf = request.EnderecoUf.Trim().ToUpperInvariant();
            query = query.Where(l => l.EnderecoUf == uf);
        }

        if (request.EstaAtivo.HasValue)
        {
            query = query.Where(l => l.EstaAtivo == request.EstaAtivo.Value);
        }

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        var ordenada = request.OrdenarPor?.ToLowerInvariant() switch
        {
            "enderecocidade" => descendente ? query.OrderByDescending(x => x.EnderecoCidade).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EnderecoCidade).ThenBy(x => x.Id),
            "salasquantidade" => descendente ? query.OrderByDescending(x => x.Salas.Count).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Salas.Count).ThenBy(x => x.Id),
            "localcapacidadetotal" => descendente ? query.OrderByDescending(x => x.Salas.Sum(s => s.SalaCapacidade)).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Salas.Sum(s => s.SalaCapacidade)).ThenBy(x => x.Id),
            _ => descendente ? query.OrderByDescending(x => x.LocalNome).ThenByDescending(x => x.Id) : query.OrderBy(x => x.LocalNome).ThenBy(x => x.Id)
        };
        var pagina = await ordenada
            .Select(l => new ListarLocaisItemResponse(l.Id, l.LocalNome, l.EnderecoCidade, l.EnderecoUf, l.Salas.Count, l.Salas.Sum(s => s.SalaCapacidade), l.EstaAtivo))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        return pagina;
    }
}
