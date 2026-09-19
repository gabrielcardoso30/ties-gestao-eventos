using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.ListarTrilhas;
internal sealed class ListarTrilhasUseCase(EventosDbContext db, ILogger<ListarTrilhasUseCase> logger) : IUseCase<ListarTrilhasRequest, IReadOnlyList<ListarTrilhasItemResponse>>
{
    public async Task<Result<IReadOnlyList<ListarTrilhasItemResponse>>> HandleAsync(ListarTrilhasRequest request, CancellationToken ct)
    {
        if (!await db.Eventos.TagWith("Eventos.ListarTrilhas.VerificarEvento").AnyAsync(e => e.Id == request.EventoId, ct))
        {
            logger.LogInformation("Listagem de trilhas rejeitada: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }
        var query = db.Trilhas.TagWith("Eventos.ListarTrilhas").AsNoTracking().Where(t => t.EventoId == request.EventoId);
        if (request.EstaAtivo.HasValue)
        {
            query = query.Where(t => t.EstaAtivo == request.EstaAtivo.Value);
            logger.LogDebug("Filtro de ativo={Active} aplicado às trilhas do evento {EventoId}", request.EstaAtivo, request.EventoId);
        }
        else
        {
            logger.LogDebug("Listagem de trilhas do evento {EventoId} inclui ativas e inativas", request.EventoId);
        }
        var trilhas = await query.OrderBy(t => t.TrilhaNome).Select(t => new ListarTrilhasItemResponse(t.Id, t.EventoId, t.TrilhaNome, t.TrilhaDescricao, t.TrilhaCor, t.EstaAtivo)).ToListAsync(ct);
        logger.LogInformation("Evento {EventoId} possui {TrackCount} trilha(s) no filtro solicitado", request.EventoId, trilhas.Count);
        return trilhas;
    }
}
