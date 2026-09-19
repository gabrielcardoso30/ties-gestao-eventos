using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Pessoas;

namespace Module.Pessoas.Shared;

/// <summary>Implementação do contrato síncrono consumido por Eventos e Palestras. Projeções mínimas, sem tracking, apenas pessoas ativas.</summary>
internal sealed class PessoasModuleApi(PessoasDbContext db) : IPessoasModuleApi
{
    public Task<PessoaResumo?> ObterPessoaResumoAsync(Guid pessoaId, CancellationToken cancellationToken) =>
        db.Pessoas
            .TagWith("Pessoas.ModuleApi.ObterPessoaResumo")
            .AsNoTracking()
            .Where(p => p.Id == pessoaId && p.EstaAtivo && p.ExcluidoEm == null)
            .Select(p => new PessoaResumo(p.Id, p.PessoaNome, p.PessoaEmail))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<PessoaResumo>> ObterPessoasResumoAsync(IReadOnlyCollection<Guid> pessoaIds, CancellationToken cancellationToken)
    {
        if (pessoaIds.Count == 0)
        {
            return [];
        }

        var ids = pessoaIds.Distinct().ToList();
        return await db.Pessoas
            .TagWith("Pessoas.ModuleApi.ObterPessoasResumo")
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id) && p.EstaAtivo && p.ExcluidoEm == null)
            .Select(p => new PessoaResumo(p.Id, p.PessoaNome, p.PessoaEmail))
            .ToListAsync(cancellationToken);
    }
}
