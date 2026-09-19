using Shared.Observability.Telemetria;

namespace Module.Identidade.Shared;

internal static class IdentidadeTelemetry
{
    public static readonly ModuleTelemetry Instance = new(IdentidadeDbContext.SchemaName);
}
