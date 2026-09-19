using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Locais;

namespace Module.Locais.Shared;

/// <summary>Implementação do contrato síncrono consumido por Eventos e Palestras. Projeções mínimas, sem tracking.</summary>
internal sealed class LocaisModuleApi(LocaisDbContext db) : ILocaisModuleApi
{
    public Task<LocalResumo?> ObterLocalResumoAsync(Guid localId, CancellationToken cancellationToken) =>
        db.Locais
            .TagWith("Locais.ModuleApi.ObterLocalResumo")
            .AsNoTracking()
            .Where(l => l.Id == localId)
            .Select(l => new LocalResumo(l.Id, l.LocalNome, l.Salas.Sum(s => s.SalaCapacidade), l.Salas.Count))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<SalaResumo?> ObterSalaResumoAsync(Guid salaId, CancellationToken cancellationToken) =>
        db.Salas
            .TagWith("Locais.ModuleApi.ObterSalaResumo")
            .AsNoTracking()
            .Where(s => s.Id == salaId && s.EstaAtivo)
            .Select(s => new SalaResumo(s.Id, s.LocalId, s.SalaNome, s.SalaCapacidade, s.SalaTipo.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
}
