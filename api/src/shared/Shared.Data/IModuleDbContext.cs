using Microsoft.EntityFrameworkCore;
using Shared.Data.Outbox;

namespace Shared.Data;

/// <summary>
/// Contrato mínimo que a infraestrutura compartilhada (interceptor de auditoria, Outbox, migrador) exige de um DbContext de módulo.
/// Implementado por <see cref="ModuleDbContext"/> e por contextos que precisam de outra base (ex.: <c>IdentityDbContext</c>).
/// </summary>
public interface IModuleDbContext
{
    /// <summary>Nome do schema = nome do módulo (ex.: "Locais").</summary>
    string Schema { get; }

    /// <summary>Permite ao módulo Auditoria desligar a auto-auditoria e evitar recursão.</summary>
    bool AuditChangesEnabled { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }
}
