using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ExcluirEvento;

/// <summary>Exclusão lógica do evento e de suas inscrições (interceptor converte Remove em soft delete).</summary>
internal sealed class ExcluirEventoUseCase(EventosDbContext db, ILogger<ExcluirEventoUseCase> logger) : IUseCase<ExcluirEventoRequest, ExcluirEventoResponse>
{
    public async Task<Result<ExcluirEventoResponse>> HandleAsync(ExcluirEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.ExcluirEvento.Carregar")
            .Include(e => e.Inscricoes)
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Evento {EventoId} não encontrado para exclusão", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }

        logger.LogDebug("Chamando agregado Evento {EventoId} para marcar exclusão; situação={EventoSituacao}, inscrições={RegistrationCount}", evento.Id, evento.EventoSituacao, evento.Inscricoes.Count);
        var resultado = evento.MarcarExcluido();
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Evento {EventoId} rejeitou exclusão pela regra {ErrorCode}", evento.Id, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Inscricoes.RemoveRange(evento.Inscricoes);
            db.Eventos.Remove(evento);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Evento {EventoId} e {RegistrationCount} inscrição(ões) excluídos logicamente", evento.Id, evento.Inscricoes.Count);
            return Result.Success(new ExcluirEventoResponse(evento.Id));
        }, cancellationToken);
    }
}
