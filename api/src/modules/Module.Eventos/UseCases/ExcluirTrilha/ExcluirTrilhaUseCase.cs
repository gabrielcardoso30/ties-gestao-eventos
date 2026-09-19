using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.ExcluirTrilha;
internal sealed class ExcluirTrilhaUseCase(EventosDbContext db) : IUseCase<ExcluirTrilhaRequest, ExcluirTrilhaResponse>
{
    public async Task<Result<ExcluirTrilhaResponse>> HandleAsync(ExcluirTrilhaRequest request, CancellationToken ct)
    {
        var evento = await db.Eventos.TagWith("Eventos.ExcluirTrilha.CarregarEvento").Include(e => e.Trilhas).FirstOrDefaultAsync(e => e.Id == request.EventoId, ct);
        if (evento is null) return EventosErros.EventoNaoEncontrado;
        var resultado = evento.RemoverTrilha(request.TrilhaId); if (resultado.IsFailure) return resultado.Error;
        return await db.ExecuteInTransactionAsync(async token => { db.Trilhas.Remove(resultado.Value); await db.SaveChangesAsync(token); return Result.Success(new ExcluirTrilhaResponse(resultado.Value.Id)); }, ct);
    }
}
