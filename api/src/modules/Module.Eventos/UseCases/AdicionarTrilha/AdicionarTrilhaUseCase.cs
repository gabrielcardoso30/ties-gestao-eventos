using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;
namespace Module.Eventos.UseCases.AdicionarTrilha;
internal sealed class AdicionarTrilhaUseCase(EventosDbContext db, ILogger<AdicionarTrilhaUseCase> logger) : IUseCase<AdicionarTrilhaRequest, AdicionarTrilhaResponse>
{
    public async Task<Result<AdicionarTrilhaResponse>> HandleAsync(AdicionarTrilhaRequest request, CancellationToken ct)
    {
        var evento = await db.Eventos.TagWith("Eventos.AdicionarTrilha.CarregarEvento").Include(e => e.Trilhas).FirstOrDefaultAsync(e => e.Id == request.EventoId, ct);
        if (evento is null)
        {
            logger.LogInformation("Adição de trilha rejeitada: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }
        logger.LogDebug("Chamando agregado Evento {EventoId} para adicionar trilha; trilhas atuais={TrackCount}", evento.Id, evento.Trilhas.Count);
        var resultado = evento.AdicionarTrilha(request.TrilhaNome, request.TrilhaDescricao, request.TrilhaCor);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Evento {EventoId} rejeitou nova trilha pela regra {ErrorCode}", evento.Id, resultado.Error.Code);
            return resultado.Error;
        }
        var trilha = resultado.Value;
        return await db.ExecuteInTransactionAsync(async token =>
        {
            await db.SaveChangesAsync(token);
            logger.LogInformation("Trilha {TrackId} adicionada ao evento {EventoId}; total de trilhas={TrackCount}", trilha.Id, evento.Id, evento.Trilhas.Count);
            return Result.Success(new AdicionarTrilhaResponse(trilha.Id, trilha.EventoId, trilha.TrilhaNome, trilha.TrilhaDescricao, trilha.TrilhaCor));
        }, ct);
    }
}
