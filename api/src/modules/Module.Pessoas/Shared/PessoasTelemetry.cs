using Shared.Observability.Telemetria;

namespace Module.Pessoas.Shared;

internal static class PessoasTelemetry
{
    public static readonly ModuleTelemetry Instance = new(PessoasDbContext.SchemaName);
}
