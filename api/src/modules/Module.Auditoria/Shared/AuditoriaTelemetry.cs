using Shared.Observability.Telemetria;

namespace Module.Auditoria.Shared;

internal static class AuditoriaTelemetry
{
    public static readonly ModuleTelemetry Instance = new(AuditoriaDbContext.SchemaName);
}
