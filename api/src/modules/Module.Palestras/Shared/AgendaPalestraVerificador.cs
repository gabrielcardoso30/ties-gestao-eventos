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
internal sealed class AgendaPalestraVerificador(IEventosModuleApi eventosApi, ILocaisModuleApi locaisApi)
{
    private static readonly string[] SituacoesQueNaoAceitamPalestras = ["Encerrado", "Cancelado"];

    public async Task<Result<EventoResumo>> VerificarAsync(Guid eventoId, Guid? salaId, DateTimeOffset palestraInicio, DateTimeOffset palestraFim, CancellationToken cancellationToken)
    {
        var evento = await eventosApi.ObterEventoResumoAsync(eventoId, cancellationToken);
        if (evento is null)
        {
            return PalestrasErros.EventoNaoEncontrado;
        }

        if (SituacoesQueNaoAceitamPalestras.Contains(evento.EventoSituacao, StringComparer.OrdinalIgnoreCase))
        {
            return PalestrasErros.EventoNaoAceitaPalestras;
        }

        if (palestraInicio < evento.EventoDataInicio || palestraFim > evento.EventoDataFim)
        {
            return PalestrasErros.PeriodoForaDoEvento;
        }

        if (salaId.HasValue)
        {
            var sala = await locaisApi.ObterSalaResumoAsync(salaId.Value, cancellationToken);
            if (sala is null)
            {
                return PalestrasErros.SalaNaoEncontrada;
            }

            if (evento.LocalId is null || sala.LocalId != evento.LocalId.Value)
            {
                return PalestrasErros.SalaNaoPertenceAoLocal;
            }
        }

        return evento;
    }
}
