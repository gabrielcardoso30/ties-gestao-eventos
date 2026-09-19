using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.ListarTrilhas;
internal sealed class ListarTrilhasUseCase(EventosDbContext db) : IUseCase<ListarTrilhasRequest, IReadOnlyList<ListarTrilhasItemResponse>>
{
    public async Task<Result<IReadOnlyList<ListarTrilhasItemResponse>>> HandleAsync(ListarTrilhasRequest request, CancellationToken ct)
    {
        if (!await db.Eventos.TagWith("Eventos.ListarTrilhas.VerificarEvento").AnyAsync(e => e.Id == request.EventoId, ct)) return EventosErros.EventoNaoEncontrado;
        var query = db.Trilhas.TagWith("Eventos.ListarTrilhas").AsNoTracking().Where(t => t.EventoId == request.EventoId);
        if (request.EstaAtivo.HasValue) query = query.Where(t => t.EstaAtivo == request.EstaAtivo.Value);
        return await query.OrderBy(t => t.TrilhaNome).Select(t => new ListarTrilhasItemResponse(t.Id, t.EventoId, t.TrilhaNome, t.TrilhaDescricao, t.TrilhaCor, t.EstaAtivo)).ToListAsync(ct);
    }
}
