using Shared.Observability.Telemetria;

namespace Module.Eventos.Shared;

internal static class EventosTelemetry
{
    public static readonly ModuleTelemetry Instance = new(EventosDbContext.SchemaName);
}
