using Shared.Contracts.Integracao;

namespace Shared.Contracts.Locais;

/// <summary>Contrato síncrono do módulo Locais consumido por outros módulos (ex.: Eventos, Palestras).</summary>
public interface ILocaisModuleApi
{
    Task<LocalResumo?> ObterLocalResumoAsync(Guid localId, CancellationToken cancellationToken);
    Task<SalaResumo?> ObterSalaResumoAsync(Guid salaId, CancellationToken cancellationToken);
}

public sealed record LocalResumo(Guid Id, string LocalNome, int LocalCapacidadeTotal, int SalasQuantidade);
public sealed record SalaResumo(Guid Id, Guid LocalId, string SalaNome, int SalaCapacidade, string SalaTipo);

public sealed record LocalCriado(Guid LocalId, string LocalNome) : IntegrationEvent;
public sealed record LocalExcluido(Guid LocalId) : IntegrationEvent;
