using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Shared.Data.Extensions;

public static class DbContextTransactionExtensions
{
    /// <summary>
    /// Executa a operação dentro de uma transação explícita, compatível com a estratégia de retry do Npgsql
    /// (falhas transitórias re-executam o bloco inteiro). Todo caso de uso de escrita usa este método.
    /// </summary>
    public static Task<TResult> ExecuteInTransactionAsync<TResult>(this DbContext db, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        var logger = db.GetService<ILoggerFactory>().CreateLogger("Shared.Data.Transaction");
        var contexto = db.GetType().Name;
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async ct =>
        {
            logger.LogDebug("Iniciando transação no contexto {DbContext}", contexto);
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var resultado = await operation(ct);
                await transaction.CommitAsync(ct);
                logger.LogInformation("Transação {TransactionId} confirmada no contexto {DbContext}", transaction.TransactionId, contexto);
                return resultado;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Transação {TransactionId} será revertida no contexto {DbContext}", transaction.TransactionId, contexto);
                throw;
            }
        }, cancellationToken);
    }
}
