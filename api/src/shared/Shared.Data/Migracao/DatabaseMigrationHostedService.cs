using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Shared.Data.Migracao;

/// <summary>
/// Aplica as migrações de todos os módulos na subida (quando <c>Database:MigrateOnStartup=true</c>).
/// Usa advisory lock do PostgreSQL para que várias instâncias subindo em paralelo não migrem ao mesmo tempo.
/// Em produção com alta criticidade, prefira executar as migrações em pipeline (ver runbook).
/// </summary>
public sealed class DatabaseMigrationHostedService(
    IServiceScopeFactory scopeFactory,
    ModuleDbContextRegistry registry,
    IConfiguration configuration,
    ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    private const long LockKey = 7_202_410; // "gestao-eventos" migrações

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("Database:MigrateOnStartup", false))
        {
            logger.LogInformation("Migração automática desabilitada (Database:MigrateOnStartup=false)");
            return;
        }

        var connectionString = configuration.GetConnectionString("GestaoEventos") ?? throw new InvalidOperationException("ConnectionStrings:GestaoEventos não configurada");
        await using var lockConnection = new NpgsqlConnection(connectionString);
        await lockConnection.OpenAsync(cancellationToken);
        await using (var cmd = new NpgsqlCommand($"SELECT pg_advisory_lock({LockKey})", lockConnection))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            foreach (var (contextType, schema) in registry.Contexts)
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
                var pendentes = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                if (pendentes.Count == 0)
                {
                    logger.LogInformation("Schema {Schema}: nenhuma migração pendente", schema);
                    continue;
                }

                logger.LogInformation("Schema {Schema}: aplicando {Quantidade} migração(ões): {Migracoes}", schema, pendentes.Count, string.Join(", ", pendentes));
                await db.Database.MigrateAsync(cancellationToken);
            }
        }
        finally
        {
            await using var cmd = new NpgsqlCommand($"SELECT pg_advisory_unlock({LockKey})", lockConnection);
            await cmd.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
