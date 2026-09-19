using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Data.Outbox;

/// <summary>
/// Implementação do Outbox por módulo. O "claim" é feito com UPDATE condicional (otimista), o que permite
/// múltiplas instâncias da API (ou um Host.Worker) processarem em paralelo sem duplicar entregas.
/// </summary>
internal sealed class OutboxStore<TContext>(IServiceScopeFactory scopeFactory, TimeProvider timeProvider) : IOutboxStore
    where TContext : ModuleDbContext
{
    public string Modulo => ModuleDbContext.ResolveModuleName(typeof(TContext));

    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var agora = timeProvider.GetUtcNow();
        var ate = agora.Add(lockDuration);

        var candidatos = await db.OutboxMessages
            .TagWith($"Outbox.{Modulo}.ClaimBatch")
            .AsNoTracking()
            .Where(m => m.ProcessedOn == null && (m.LockedUntil == null || m.LockedUntil < agora))
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (candidatos.Count == 0)
        {
            return [];
        }

        // Claim otimista: só ganha quem ainda vir a mensagem livre.
        await db.OutboxMessages
            .TagWith($"Outbox.{Modulo}.Lock")
            .Where(m => candidatos.Contains(m.Id) && m.ProcessedOn == null && (m.LockedUntil == null || m.LockedUntil < agora))
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.LockedUntil, ate), cancellationToken);

        return await db.OutboxMessages
            .TagWith($"Outbox.{Modulo}.ReadClaimed")
            .AsNoTracking()
            .Where(m => candidatos.Contains(m.Id) && m.LockedUntil == ate)
            .OrderBy(m => m.OccurredOn)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var agora = timeProvider.GetUtcNow();
        await db.OutboxMessages
            .TagWith($"Outbox.{Modulo}.MarkProcessed")
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.ProcessedOn, agora)
                .SetProperty(m => m.LockedUntil, (DateTimeOffset?)null)
                .SetProperty(m => m.Error, (string?)null), cancellationToken);
    }

    public async Task MarkFailedAsync(Guid messageId, string error, TimeSpan retryDelay, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var proximaTentativa = timeProvider.GetUtcNow().Add(retryDelay);
        var erro = error.Length > 2000 ? error[..2000] : error;
        await db.OutboxMessages
            .TagWith($"Outbox.{Modulo}.MarkFailed")
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Attempts, m => m.Attempts + 1)
                .SetProperty(m => m.LockedUntil, proximaTentativa)
                .SetProperty(m => m.Error, erro), cancellationToken);
    }

    public async Task<int> CountPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        return await db.OutboxMessages
            .TagWith($"Outbox.{Modulo}.CountPending")
            .CountAsync(m => m.ProcessedOn == null, cancellationToken);
    }
}
