namespace Shared.Contracts.Integracao;

/// <summary>
/// Contrato assíncrono entre módulos. Publicado via Outbox (mesma transação do agregado) e
/// entregue in-process hoje; amanhã, por um broker, sem alterar os módulos.
/// </summary>
public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTimeOffset OcorridoEm { get; }
}

/// <summary>Base conveniente para eventos de integração (records imutáveis).</summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OcorridoEm { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Manipulador de um evento de integração. Cada módulo registra os seus.</summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}
