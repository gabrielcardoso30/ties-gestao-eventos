using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ListarEventos;

internal sealed class ListarEventosUseCase(EventosDbContext db) : IUseCase<ListarEventosRequest, PagedResult<ListarEventosItemResponse>>
{
    public async Task<Result<PagedResult<ListarEventosItemResponse>>> HandleAsync(ListarEventosRequest request, CancellationToken cancellationToken)
    {
        var query = db.Eventos.TagWith("Eventos.ListarEventos").AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(e => EF.Functions.ILike(e.EventoNome, busca) || (e.EventoDescricao != null && EF.Functions.ILike(e.EventoDescricao, busca)));
        }

        if (request.EventoSituacao.HasValue)
        {
            query = query.Where(e => e.EventoSituacao == request.EventoSituacao.Value);
        }

        if (request.EventoFormato.HasValue)
        {
            query = query.Where(e => e.EventoFormato == request.EventoFormato.Value);
        }

        if (request.DataInicioDe.HasValue)
        {
            query = query.Where(e => e.EventoDataInicio >= request.DataInicioDe.Value);
        }

        if (request.DataInicioAte.HasValue)
        {
            query = query.Where(e => e.EventoDataInicio <= request.DataInicioAte.Value);
        }

        var pagina = await query
            .OrderByDescending(e => e.EventoDataInicio)
            .ThenBy(e => e.EventoNome)
            .Select(e => new ListarEventosItemResponse(
                e.Id, e.EventoNome, e.EventoDataInicio, e.EventoDataFim, e.EventoFormato, e.EventoSituacao, e.LocalId,
                e.Inscricoes.Count(i => i.InscricaoSituacao == InscricaoSituacao.Confirmada)))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        return pagina;
    }
}
