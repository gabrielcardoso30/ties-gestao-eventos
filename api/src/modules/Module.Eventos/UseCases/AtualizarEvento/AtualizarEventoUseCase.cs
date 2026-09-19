using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Locais;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.AtualizarEvento;

internal sealed class AtualizarEventoUseCase(EventosDbContext db, ILocaisModuleApi locais, ILogger<AtualizarEventoUseCase> logger) : IUseCase<AtualizarEventoRequest, AtualizarEventoResponse>
{
    public async Task<Result<AtualizarEventoResponse>> HandleAsync(AtualizarEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.AtualizarEvento.Carregar")
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Evento {EventoId} não encontrado para atualização", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }

        if (!evento.PodeSerAlterado)
        {
            logger.LogInformation("Atualização do evento {EventoId} rejeitada porque a situação {EventoSituacao} não permite alteração", evento.Id, evento.EventoSituacao);
            return EventosErros.EventoNaoPodeSerAlterado;
        }

        logger.LogDebug("Atualização do evento {EventoId} referencia local={HasLocal}", evento.Id, request.LocalId.HasValue);
        if (request.LocalId.HasValue)
        {
            logger.LogDebug("Consultando módulo Locais para validar local {LocalId} do evento {EventoId}", request.LocalId, evento.Id);
            var local = await locais.ObterLocalResumoAsync(request.LocalId.Value, cancellationToken);
            if (local is null)
            {
                logger.LogInformation("Atualização do evento {EventoId} rejeitada: local {LocalId} não encontrado", evento.Id, request.LocalId);
                return EventosErros.LocalNaoEncontrado;
            }
        }

        logger.LogDebug("Chamando agregado Evento {EventoId} para atualizar dados e formato {EventoFormato}", evento.Id, request.EventoFormato);
        var resultado = evento.Atualizar(
            request.EventoNome, request.EventoDescricao, request.EventoDataInicio, request.EventoDataFim,
            request.EventoFormato, request.LocalId, request.EventoLinkRemoto, request.EventoCapacidadeMaxima);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Evento {EventoId} rejeitou atualização pela regra {ErrorCode}", evento.Id, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Evento {EventoId} atualizado em situação {EventoSituacao}", evento.Id, evento.EventoSituacao);
            return Result.Success(new AtualizarEventoResponse(evento.Id, evento.EventoNome, evento.EventoSituacao, evento.AlteradoEm));
        }, cancellationToken);
    }
}
