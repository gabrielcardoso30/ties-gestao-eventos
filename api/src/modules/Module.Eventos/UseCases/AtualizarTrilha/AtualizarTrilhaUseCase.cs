using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.AtualizarTrilha;
internal sealed class AtualizarTrilhaUseCase(EventosDbContext db, ILogger<AtualizarTrilhaUseCase> logger) : IUseCase<AtualizarTrilhaRequest, AtualizarTrilhaResponse>
{
    public async Task<Result<AtualizarTrilhaResponse>> HandleAsync(AtualizarTrilhaRequest request, CancellationToken ct)
    {
        var evento = await db.Eventos.TagWith("Eventos.AtualizarTrilha.CarregarEvento").Include(e => e.Trilhas).FirstOrDefaultAsync(e => e.Id == request.EventoId, ct);
        if (evento is null)
        {
            logger.LogInformation("Atualização de trilha rejeitada: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }
        logger.LogDebug("Chamando agregado Evento {EventoId} para atualizar trilha {TrackId}", evento.Id, request.TrilhaId);
        var resultado = evento.AtualizarTrilha(request.TrilhaId, request.TrilhaNome, request.TrilhaDescricao, request.TrilhaCor);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Evento {EventoId} rejeitou atualização da trilha {TrackId} pela regra {ErrorCode}", evento.Id, request.TrilhaId, resultado.Error.Code);
            return resultado.Error;
        }
        var trilha = evento.Trilhas.First(t => t.Id == request.TrilhaId);
        trilha.EstaAtivo = request.EstaAtivo;
        logger.LogDebug("Estado ativo da trilha {TrackId} definido como {TrackActive}", trilha.Id, trilha.EstaAtivo);
        return await db.ExecuteInTransactionAsync(async token =>
        {
            await db.SaveChangesAsync(token);
            logger.LogInformation("Trilha {TrackId} do evento {EventoId} atualizada", trilha.Id, evento.Id);
            return Result.Success(new AtualizarTrilhaResponse(trilha.Id, trilha.EventoId, trilha.TrilhaNome, trilha.TrilhaDescricao, trilha.TrilhaCor, trilha.EstaAtivo));
        }, ct);
    }
}
