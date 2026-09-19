using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Shared.Observability.Telemetria;

/// <summary>
/// Fonte de traces e métricas por módulo. Convenção de nomes <c>GestaoEventos.{Modulo}</c>, capturados por wildcard na
/// configuração do OpenTelemetry. Uso: <c>using var atividade = LocaisTelemetry.Instance.StartActivity("CriarLocal");</c>
/// </summary>
public sealed class ModuleTelemetry
{
    public const string Prefixo = "GestaoEventos";

    public ModuleTelemetry(string modulo)
    {
        Modulo = modulo;
        ActivitySource = new ActivitySource($"{Prefixo}.{modulo}");
        Meter = new Meter($"{Prefixo}.{modulo}");
        UseCasesExecutados = Meter.CreateCounter<long>("usecase.executions", description: "Casos de uso executados");
        UseCasesFalhas = Meter.CreateCounter<long>("usecase.failures", description: "Casos de uso com falha de negócio");
        UseCaseDuracao = Meter.CreateHistogram<double>("usecase.duration", unit: "ms", description: "Duração dos casos de uso");
    }

    public string Modulo { get; }
    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }
    public Counter<long> UseCasesExecutados { get; }
    public Counter<long> UseCasesFalhas { get; }
    public Histogram<double> UseCaseDuracao { get; }

    public Activity? StartActivity(string useCase) => ActivitySource.StartActivity($"{Modulo}.{useCase}", ActivityKind.Internal);

    public void RegistrarExecucao(string useCase, bool sucesso, double duracaoMs, string? codigoErro = null)
    {
        var tags = new TagList { { "usecase", useCase }, { "module", Modulo } };
        UseCasesExecutados.Add(1, tags);
        UseCaseDuracao.Record(duracaoMs, tags);
        if (!sucesso)
        {
            tags.Add("error.code", codigoErro);
            UseCasesFalhas.Add(1, tags);
        }
    }
}
