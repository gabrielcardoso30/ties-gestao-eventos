using System.Data.Common;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Shared.Data.Interceptors;

/// <summary>
/// Regra do DBA: toda consulta precisa de <c>TagWith("Modulo.UseCase")</c> (vira um comentário SQL, visível em pg_stat_activity).
/// O interceptor não bloqueia, mas registra warning e métrica para consultas sem tag, tornando a regra mensurável.
/// </summary>
public sealed class QueryTagInterceptor(ILogger<QueryTagInterceptor> logger) : DbCommandInterceptor
{
    public static readonly Meter Meter = new("GestaoEventos.Data");
    private static readonly Counter<long> ConsultasSemTag = Meter.CreateCounter<long>("db.queries.untagged", description: "Consultas SELECT executadas sem TagWith");

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Verificar(command, eventData);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Verificar(command, eventData);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        RegistrarFalha(command, eventData);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        RegistrarFalha(command, eventData);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void Verificar(DbCommand command, CommandEventData eventData)
    {
        var sql = command.CommandText;
        if (sql.StartsWith("-- ", StringComparison.Ordinal) || sql.Contains("__EFMigrationsHistory", StringComparison.Ordinal) || !sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var contexto = eventData.Context?.GetType().Name ?? "desconhecido";
        ConsultasSemTag.Add(1, new KeyValuePair<string, object?>("db.context", contexto));
        logger.LogWarning("Consulta sem TagWith detectada no contexto {DbContext}: {Sql}", contexto, sql.Length > 300 ? sql[..300] : sql);
    }

    private void RegistrarFalha(DbCommand command, CommandErrorEventData eventData)
    {
        logger.LogError(
            eventData.Exception,
            "Banco falhou ao executar {DatabaseOperation} no contexto {DbContext} após {DurationMs:0.0} ms",
            ObterOperacao(command), eventData.Context?.GetType().Name ?? "desconhecido", eventData.Duration.TotalMilliseconds);
    }

    private static string ObterOperacao(DbCommand command)
    {
        var primeiraLinha = command.CommandText.AsSpan().TrimStart();
        if (primeiraLinha.StartsWith("-- ", StringComparison.Ordinal))
        {
            primeiraLinha = primeiraLinha[3..];
            var fim = primeiraLinha.IndexOfAny('\r', '\n');
            return (fim >= 0 ? primeiraLinha[..fim] : primeiraLinha).Trim().ToString();
        }

        return command.CommandText.AsSpan().TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
            ? "ConsultaSemTag"
            : "PersistenciaEF";
    }
}
