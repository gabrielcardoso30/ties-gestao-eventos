using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Palestras;

namespace Module.Palestras.Shared;

/// <summary>Implementação do contrato síncrono consumido por Eventos (publicar exige ao menos uma palestra). Sem tracking.</summary>
internal sealed class PalestrasModuleApi(PalestrasDbContext db) : IPalestrasModuleApi
{
    public Task<int> ContarPalestrasDoEventoAsync(Guid eventoId, CancellationToken cancellationToken) =>
        db.Palestras
            .TagWith("Palestras.ModuleApi.ContarPalestrasDoEvento")
            .AsNoTracking()
            .CountAsync(p => p.EventoId == eventoId && p.EstaAtivo, cancellationToken);
}
