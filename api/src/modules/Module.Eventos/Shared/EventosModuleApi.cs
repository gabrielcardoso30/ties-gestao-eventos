using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Shared.Contracts.Eventos;

namespace Module.Eventos.Shared;

/// <summary>Implementação do contrato síncrono consumido por Palestras. Projeções mínimas, sem tracking; enums como texto.</summary>
internal sealed class EventosModuleApi(EventosDbContext db) : IEventosModuleApi
{
    public Task<EventoResumo?> ObterEventoResumoAsync(Guid eventoId, CancellationToken cancellationToken) =>
        db.Eventos
            .TagWith("Eventos.ModuleApi.ObterEventoResumo")
            .AsNoTracking()
            .Where(e => e.Id == eventoId)
            .Select(e => new EventoResumo(
                e.Id, e.EventoNome, e.EventoDataInicio, e.EventoDataFim, e.EventoFormato.ToString(), e.EventoSituacao.ToString(), e.LocalId))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> InscricaoConfirmadaExisteAsync(Guid eventoId, Guid pessoaId, CancellationToken cancellationToken) =>
        db.Inscricoes
            .TagWith("Eventos.ModuleApi.InscricaoConfirmadaExiste")
            .AsNoTracking()
            .AnyAsync(i => i.EventoId == eventoId && i.PessoaId == pessoaId && i.InscricaoSituacao == InscricaoSituacao.Confirmada, cancellationToken);

    public Task<TrilhaResumo?> ObterTrilhaResumoAsync(Guid eventoId, Guid trilhaId, CancellationToken cancellationToken) =>
        db.Trilhas.TagWith("Eventos.ModuleApi.ObterTrilhaResumo").AsNoTracking()
            .Where(t => t.Id == trilhaId && t.EventoId == eventoId)
            .Select(t => new TrilhaResumo(t.Id, t.EventoId, t.TrilhaNome, t.EstaAtivo))
            .FirstOrDefaultAsync(cancellationToken);
}
