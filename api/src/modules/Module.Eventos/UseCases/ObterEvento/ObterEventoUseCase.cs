using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Locais;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ObterEvento;

internal sealed class ObterEventoUseCase(EventosDbContext db, ILocaisModuleApi locais, ILogger<ObterEventoUseCase> logger) : IUseCase<ObterEventoRequest, ObterEventoResponse>
{
    public async Task<Result<ObterEventoResponse>> HandleAsync(ObterEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.ObterEvento")
            .AsNoTracking()
            .Where(e => e.Id == request.EventoId)
            .Select(e => new ObterEventoResponse(
                e.Id, e.EventoNome, e.EventoDescricao, e.EventoDataInicio, e.EventoDataFim, e.EventoFormato, e.EventoSituacao,
                e.LocalId, null, e.EventoLinkRemoto, e.EventoCapacidadeMaxima,
                e.Inscricoes.Count(i => i.InscricaoSituacao == InscricaoSituacao.Confirmada),
                e.EventoCancelamentoMotivo, e.CriadoEm, e.AlteradoEm,
                e.Trilhas.OrderBy(t => t.TrilhaNome).Select(t => new ObterEventoTrilhaResponse(t.Id, t.TrilhaNome, t.TrilhaDescricao, t.TrilhaCor, t.EstaAtivo)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Evento {EventoId} não encontrado para detalhamento", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }

        if (evento.LocalId is null)
        {
            logger.LogInformation("Evento {EventoId} carregado sem local, com {RegistrationCount} inscrição(ões) confirmada(s) e {TrackCount} trilha(s)", evento.Id, evento.InscricoesConfirmadas, evento.Trilhas.Count);
            return evento;
        }

        logger.LogDebug("Consultando módulo Locais para enriquecer evento {EventoId} com local {LocalId}", evento.Id, evento.LocalId);
        var local = await locais.ObterLocalResumoAsync(evento.LocalId.Value, cancellationToken);
        if (local is null)
        {
            logger.LogWarning("Local {LocalId} referenciado pelo evento {EventoId} não foi encontrado durante o enriquecimento", evento.LocalId, evento.Id);
        }
        else
        {
            logger.LogInformation("Evento {EventoId} enriquecido com local {LocalId}; inscrições={RegistrationCount}, trilhas={TrackCount}", evento.Id, local.Id, evento.InscricoesConfirmadas, evento.Trilhas.Count);
        }
        return evento with { LocalNome = local?.LocalNome };
    }
}
