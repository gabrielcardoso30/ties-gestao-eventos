using Shared.Contracts.Integracao;

namespace Shared.Data.Entidades;

/// <summary>Entidade que acumula eventos de integração para serem gravados no Outbox na mesma transação.</summary>
public interface IEmissorDeEventos
{
    IReadOnlyCollection<IIntegrationEvent> Eventos { get; }
    void LimparEventos();
}
