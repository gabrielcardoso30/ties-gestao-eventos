using Microsoft.EntityFrameworkCore;

namespace Shared.Data.Extensions;

public static class DbContextTransactionExtensions
{
    /// <summary>
    /// Executa a operação dentro de uma transação explícita, compatível com a estratégia de retry do Npgsql
    /// (falhas transitórias re-executam o bloco inteiro). Todo caso de uso de escrita usa este método.
    /// </summary>
    public static Task<TResult> ExecuteInTransactionAsync<TResult>(this DbContext db, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var resultado = await operation(ct);
            await transaction.CommitAsync(ct);
            return resultado;
        }, cancellationToken);
    }
}
