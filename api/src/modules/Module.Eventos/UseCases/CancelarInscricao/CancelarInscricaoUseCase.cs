using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.CancelarInscricao;

/// <summary>Cancelamento não é soft delete: a inscrição permanece com situação <c>Cancelada</c> e data de cancelamento.</summary>
internal sealed class CancelarInscricaoUseCase(EventosDbContext db, TimeProvider timeProvider, ILogger<CancelarInscricaoUseCase> logger) : IUseCase<CancelarInscricaoRequest, CancelarInscricaoResponse>
{
    public async Task<Result<CancelarInscricaoResponse>> HandleAsync(CancelarInscricaoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.CancelarInscricao.Carregar")
            .Include(e => e.Inscricoes.Where(i => i.Id == request.InscricaoId))
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Cancelamento de inscrição rejeitado: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }

        logger.LogDebug("Chamando agregado Evento {EventoId} para cancelar inscrição {RegistrationId}", evento.Id, request.InscricaoId);
        var resultado = evento.CancelarInscricao(request.InscricaoId, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Evento {EventoId} rejeitou cancelamento da inscrição {RegistrationId} pela regra {ErrorCode}", evento.Id, request.InscricaoId, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Inscrição {RegistrationId} do evento {EventoId} cancelada", resultado.Value.Id, evento.Id);
            return Result.Success(new CancelarInscricaoResponse(resultado.Value.Id));
        }, cancellationToken);
    }
}
