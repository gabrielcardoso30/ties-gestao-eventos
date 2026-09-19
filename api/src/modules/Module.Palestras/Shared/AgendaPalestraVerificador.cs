using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Shared.Contracts.Eventos;
using Shared.Contracts.Locais;
using Shared.Http.Results;

namespace Module.Palestras.Shared;

/// <summary>
/// Regras cruzadas de agenda compartilhadas por criar/atualizar palestra, resolvidas via contratos de outros módulos:
/// evento existe e aceita palestras, período dentro do evento e sala pertencente ao local do evento.
/// A sobreposição de horários na sala é verificada pelo caso de uso (consulta ao próprio schema).
/// </summary>
internal sealed class AgendaPalestraVerificador(
    IEventosModuleApi eventosApi,
    ILocaisModuleApi locaisApi,
    ILogger<AgendaPalestraVerificador> logger)
{
    private static readonly string[] SituacoesQueNaoAceitamPalestras = ["Encerrado", "Cancelado"];

    public async Task<Result<EventoResumo>> VerificarAsync(Guid eventoId, Guid trilhaId, Guid? salaId, DateTimeOffset palestraInicio, DateTimeOffset palestraFim, CancellationToken cancellationToken)
    {
        logger.LogDebug("Consultando evento {EventId} para validar agenda da palestra na trilha {TrackId}", eventoId, trilhaId);
        var evento = await eventosApi.ObterEventoResumoAsync(eventoId, cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Agenda da palestra rejeitada porque o evento {EventId} não foi encontrado", eventoId);
            return PalestrasErros.EventoNaoEncontrado;
        }

        if (SituacoesQueNaoAceitamPalestras.Contains(evento.EventoSituacao, StringComparer.OrdinalIgnoreCase))
        {
            logger.LogInformation("Agenda da palestra rejeitada porque o evento {EventId} está na situação {EventStatus}", eventoId, evento.EventoSituacao);
            return PalestrasErros.EventoNaoAceitaPalestras;
        }

        logger.LogDebug("Evento {EventId} aceita palestras; consultando trilha {TrackId}", eventoId, trilhaId);
        var trilha = await eventosApi.ObterTrilhaResumoAsync(eventoId, trilhaId, cancellationToken);
        if (trilha is null)
        {
            logger.LogInformation("Agenda da palestra rejeitada porque a trilha {TrackId} não foi encontrada no evento {EventId}", trilhaId, eventoId);
            return PalestrasErros.TrilhaNaoEncontrada;
        }

        if (!trilha.EstaAtivo)
        {
            logger.LogInformation("Agenda da palestra rejeitada porque a trilha {TrackId} do evento {EventId} está inativa", trilhaId, eventoId);
            return PalestrasErros.TrilhaNaoEncontrada;
        }

        if (palestraInicio < evento.EventoDataInicio || palestraFim > evento.EventoDataFim)
        {
            logger.LogInformation("Agenda da palestra rejeitada porque o período está fora da janela do evento {EventId}", eventoId);
            return PalestrasErros.PeriodoForaDoEvento;
        }

        if (salaId.HasValue)
        {
            logger.LogDebug("Consultando sala {RoomId} para validar vínculo com o local do evento {EventId}", salaId.Value, eventoId);
            var sala = await locaisApi.ObterSalaResumoAsync(salaId.Value, cancellationToken);
            if (sala is null)
            {
                logger.LogInformation("Agenda da palestra rejeitada porque a sala {RoomId} não foi encontrada", salaId.Value);
                return PalestrasErros.SalaNaoEncontrada;
            }

            if (evento.LocalId is null)
            {
                logger.LogInformation("Agenda da palestra rejeitada porque o evento {EventId} não possui local para receber a sala {RoomId}", eventoId, salaId.Value);
                return PalestrasErros.SalaNaoPertenceAoLocal;
            }

            if (sala.LocalId != evento.LocalId.Value)
            {
                logger.LogInformation("Agenda da palestra rejeitada porque a sala {RoomId} não pertence ao local do evento {EventId}", salaId.Value, eventoId);
                return PalestrasErros.SalaNaoPertenceAoLocal;
            }

            logger.LogDebug("Sala {RoomId} pertence ao local do evento {EventId}", salaId.Value, eventoId);
        }
        else
        {
            logger.LogDebug("Validação da agenda da palestra não exige sala para o evento {EventId}", eventoId);
        }

        logger.LogDebug("Agenda da palestra validada para o evento {EventId} e trilha {TrackId}", eventoId, trilhaId);
        return evento;
    }
}
