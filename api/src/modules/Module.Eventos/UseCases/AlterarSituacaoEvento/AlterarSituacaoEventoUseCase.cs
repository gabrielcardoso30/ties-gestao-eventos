using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Palestras;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.AlterarSituacaoEvento;

/// <summary>Aplica a máquina de estados do agregado. Publicar consulta o módulo Palestras (ao menos uma palestra).</summary>
internal sealed class AlterarSituacaoEventoUseCase(EventosDbContext db, IPalestrasModuleApi palestras, ILogger<AlterarSituacaoEventoUseCase> logger) : IUseCase<AlterarSituacaoEventoRequest, AlterarSituacaoEventoResponse>
{
    public async Task<Result<AlterarSituacaoEventoResponse>> HandleAsync(AlterarSituacaoEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.AlterarSituacaoEvento.Carregar")
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Evento {EventoId} não encontrado para transição para {TargetStatus}", request.EventoId, request.EventoSituacao);
            return EventosErros.EventoNaoEncontrado;
        }

        logger.LogInformation("Avaliando transição do evento {EventoId} de {CurrentStatus} para {TargetStatus}", evento.Id, evento.EventoSituacao, request.EventoSituacao);

        Result resultado;
        switch (request.EventoSituacao)
        {
            case EventoSituacao.Publicado:
                logger.LogDebug("Chamando fluxo de domínio para publicar evento {EventoId}", evento.Id);
                resultado = await PublicarAsync(evento, cancellationToken);
                break;
            case EventoSituacao.EmAndamento:
                logger.LogDebug("Chamando agregado Evento {EventoId}.Iniciar", evento.Id);
                resultado = evento.Iniciar();
                break;
            case EventoSituacao.Encerrado:
                logger.LogDebug("Chamando agregado Evento {EventoId}.Encerrar", evento.Id);
                resultado = evento.Encerrar();
                break;
            case EventoSituacao.Cancelado:
                logger.LogDebug("Chamando agregado Evento {EventoId}.Cancelar; motivo informado={HasReason}", evento.Id, !string.IsNullOrWhiteSpace(request.Motivo));
                resultado = evento.Cancelar(request.Motivo);
                break;
            default:
                logger.LogInformation("Transição solicitada para situação não suportada no evento {EventoId}", evento.Id);
                resultado = Result.Failure(EventosErros.TransicaoSituacaoInvalida);
                break;
        }
        if (resultado.IsFailure)
        {
            logger.LogInformation("Transição do evento {EventoId} rejeitada pela regra {ErrorCode}", evento.Id, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Evento {EventoId} persistido na situação {EventoSituacao}", evento.Id, evento.EventoSituacao);
            return Result.Success(new AlterarSituacaoEventoResponse(evento.Id, evento.EventoSituacao));
        }, cancellationToken);
    }

    private async Task<Result> PublicarAsync(Evento evento, CancellationToken cancellationToken)
    {
        if (evento.EventoSituacao != EventoSituacao.Rascunho)
        {
            logger.LogInformation("Publicação do evento {EventoId} rejeitada porque a situação atual é {CurrentStatus}", evento.Id, evento.EventoSituacao);
            return EventosErros.TransicaoSituacaoInvalida;
        }

        logger.LogDebug("Consultando módulo Palestras para validar a publicação do evento {EventoId}", evento.Id);
        var quantidadePalestras = await palestras.ContarPalestrasDoEventoAsync(evento.Id, cancellationToken);
        logger.LogInformation("Evento {EventoId} possui {TalkCount} palestra(s) para avaliação da publicação", evento.Id, quantidadePalestras);
        logger.LogDebug("Chamando agregado Evento {EventoId}.Publicar com {TalkCount} palestra(s)", evento.Id, quantidadePalestras);
        return evento.Publicar(quantidadePalestras);
    }
}
