namespace Shared.Data.Outbox;

/// <summary>Acesso ao Outbox de um módulo, usado pelo processador em Shared.Messaging.</summary>
public interface IOutboxStore
{
    string Modulo { get; }
    Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken);
    Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid messageId, string error, TimeSpan retryDelay, CancellationToken cancellationToken);
    Task<int> CountPendingAsync(CancellationToken cancellationToken);
}
