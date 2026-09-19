using Shared.Observability.Telemetria;

namespace Module.Palestras.Shared;

internal static class PalestrasTelemetry
{
    public static readonly ModuleTelemetry Instance = new(PalestrasDbContext.SchemaName);
}
