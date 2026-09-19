using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.AdicionarTrilha;
internal sealed class AdicionarTrilhaUseCase(EventosDbContext db) : IUseCase<AdicionarTrilhaRequest, AdicionarTrilhaResponse>
{
    public async Task<Result<AdicionarTrilhaResponse>> HandleAsync(AdicionarTrilhaRequest request, CancellationToken ct)
    {
        var evento = await db.Eventos.TagWith("Eventos.AdicionarTrilha.CarregarEvento").Include(e => e.Trilhas).FirstOrDefaultAsync(e => e.Id == request.EventoId, ct);
        if (evento is null) return EventosErros.EventoNaoEncontrado;
        var resultado = evento.AdicionarTrilha(request.TrilhaNome, request.TrilhaDescricao, request.TrilhaCor);
        if (resultado.IsFailure) return resultado.Error;
        var trilha = resultado.Value;
        return await db.ExecuteInTransactionAsync(async token => { await db.SaveChangesAsync(token); return Result.Success(new AdicionarTrilhaResponse(trilha.Id, trilha.EventoId, trilha.TrilhaNome, trilha.TrilhaDescricao, trilha.TrilhaCor)); }, ct);
    }
}
