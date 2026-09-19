using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.AtualizarTrilha;
internal sealed class AtualizarTrilhaUseCase(EventosDbContext db) : IUseCase<AtualizarTrilhaRequest, AtualizarTrilhaResponse>
{
    public async Task<Result<AtualizarTrilhaResponse>> HandleAsync(AtualizarTrilhaRequest request, CancellationToken ct)
    {
        var evento = await db.Eventos.TagWith("Eventos.AtualizarTrilha.CarregarEvento").Include(e => e.Trilhas).FirstOrDefaultAsync(e => e.Id == request.EventoId, ct);
        if (evento is null) return EventosErros.EventoNaoEncontrado;
        var resultado = evento.AtualizarTrilha(request.TrilhaId, request.TrilhaNome, request.TrilhaDescricao, request.TrilhaCor);
        if (resultado.IsFailure) return resultado.Error;
        var trilha = evento.Trilhas.First(t => t.Id == request.TrilhaId); trilha.EstaAtivo = request.EstaAtivo;
        return await db.ExecuteInTransactionAsync(async token => { await db.SaveChangesAsync(token); return Result.Success(new AtualizarTrilhaResponse(trilha.Id, trilha.EventoId, trilha.TrilhaNome, trilha.TrilhaDescricao, trilha.TrilhaCor, trilha.EstaAtivo)); }, ct);
    }
}
