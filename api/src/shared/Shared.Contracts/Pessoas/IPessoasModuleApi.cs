using Shared.Contracts.Integracao;

namespace Shared.Contracts.Pessoas;

/// <summary>Contrato síncrono do módulo Pessoas (palestrantes e participantes são Pessoas).</summary>
public interface IPessoasModuleApi
{
    Task<PessoaResumo?> ObterPessoaResumoAsync(Guid pessoaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PessoaResumo>> ObterPessoasResumoAsync(IReadOnlyCollection<Guid> pessoaIds, CancellationToken cancellationToken);
}

public sealed record PessoaResumo(Guid Id, string PessoaNome, string PessoaEmail);

public sealed record PessoaCriada(Guid PessoaId, string PessoaNome, string PessoaEmail) : IntegrationEvent;
public sealed record PessoaExcluida(Guid PessoaId) : IntegrationEvent;
