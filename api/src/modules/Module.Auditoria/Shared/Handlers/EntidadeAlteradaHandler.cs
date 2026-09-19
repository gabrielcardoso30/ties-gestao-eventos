using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auditoria.Domain;
using Shared.Contracts.Auditoria;
using Shared.Contracts.Integracao;
using Shared.Data.Extensions;

namespace Module.Auditoria.Shared.Handlers;

/// <summary>
/// Consome <see cref="EntidadeAlterada"/> entregue pelo processador do Outbox e persiste o registro imutável.
/// Idempotente: o Id do evento é a chave primária; reentregas (retry, múltiplas instâncias) são ignoradas.
/// </summary>
internal sealed class EntidadeAlteradaHandler(AuditoriaDbContext db, TimeProvider timeProvider, ILogger<EntidadeAlteradaHandler> logger)
    : IIntegrationEventHandler<EntidadeAlterada>
{
    public async Task HandleAsync(EntidadeAlterada integrationEvent, CancellationToken cancellationToken)
    {
        var jaRegistrado = await db.RegistrosAuditoria
            .TagWith("Auditoria.EntidadeAlteradaHandler.VerificarIdempotencia")
            .AnyAsync(r => r.Id == integrationEvent.Id, cancellationToken);
        if (jaRegistrado)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Registro de auditoria {RegistroId} já existe; evento ignorado", integrationEvent.Id);
            }

            return;
        }

        var registro = RegistroAuditoria.Criar(integrationEvent, timeProvider.GetUtcNow());
        await db.ExecuteInTransactionAsync(async ct =>
        {
            db.RegistrosAuditoria.Add(registro);
            return await db.SaveChangesAsync(ct);
        }, cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Auditoria registrada: {Modulo}.{EntidadeNome} {EntidadeId} {Operacao} ({RegistroId})",
                registro.Modulo, registro.EntidadeNome, registro.EntidadeId, registro.Operacao, registro.Id);
        }
    }
}
