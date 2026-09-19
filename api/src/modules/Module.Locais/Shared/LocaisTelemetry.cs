using Shared.Observability.Telemetria;

namespace Module.Locais.Shared;

internal static class LocaisTelemetry
{
    public static readonly ModuleTelemetry Instance = new(LocaisDbContext.SchemaName);
}
