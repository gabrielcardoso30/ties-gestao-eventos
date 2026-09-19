using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.ExcluirTrilha;
internal sealed class ExcluirTrilhaUseCase(EventosDbContext db, ILogger<ExcluirTrilhaUseCase> logger) : IUseCase<ExcluirTrilhaRequest, ExcluirTrilhaResponse>
{
    public async Task<Result<ExcluirTrilhaResponse>> HandleAsync(ExcluirTrilhaRequest request, CancellationToken ct)
    {
        var evento = await db.Eventos.TagWith("Eventos.ExcluirTrilha.CarregarEvento").Include(e => e.Trilhas).FirstOrDefaultAsync(e => e.Id == request.EventoId, ct);
        if (evento is null)
        {
            logger.LogInformation("Exclusão de trilha rejeitada: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }
        logger.LogDebug("Chamando agregado Evento {EventoId} para remover trilha {TrackId}; trilhas atuais={TrackCount}", evento.Id, request.TrilhaId, evento.Trilhas.Count);
        var resultado = evento.RemoverTrilha(request.TrilhaId);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Evento {EventoId} rejeitou remoção da trilha {TrackId} pela regra {ErrorCode}", evento.Id, request.TrilhaId, resultado.Error.Code);
            return resultado.Error;
        }
        return await db.ExecuteInTransactionAsync(async token =>
        {
            db.Trilhas.Remove(resultado.Value);
            await db.SaveChangesAsync(token);
            logger.LogInformation("Trilha {TrackId} removida do evento {EventoId}; trilhas restantes={TrackCount}", resultado.Value.Id, evento.Id, evento.Trilhas.Count);
            return Result.Success(new ExcluirTrilhaResponse(resultado.Value.Id));
        }, ct);
    }
}
