using System.Text.Json;
using Shouldly;
using Tests.Integration.Infra;
using Xunit;

namespace Tests.Integration.Contratos;

[Collection(ApiCollection.Nome)]
public sealed class OpenApiContractTests(ApiFactory factory)
{
    [Fact]
    public async Task Contrato_yaml_versionado_deve_conter_todas_as_operacoes_expostas_em_runtime()
    {
        var client = factory.CreateClient();
        using var runtime = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        var operacoesRuntime = runtime.RootElement.GetProperty("paths").EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject()
                .Where(operation => MetodosHttp.Contains(operation.Name))
                .Select(operation => $"{operation.Name.ToUpperInvariant()} {path.Name}"))
            .ToHashSet(StringComparer.Ordinal);

        var yamlPath = Path.Combine(AppContext.BaseDirectory, "contracts", "v1", "openapi.yaml");
        var operacoesVersionadas = LerOperacoesYaml(await File.ReadAllLinesAsync(yamlPath));

        operacoesVersionadas.ShouldBe(operacoesRuntime, ignoreOrder: true);
    }

    [Fact]
    public async Task Contrato_deve_documentar_problem_details_e_uma_tag_unica_por_operacao()
    {
        var client = factory.CreateClient();
        using var runtime = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        var root = runtime.RootElement;
        root.GetProperty("components").GetProperty("schemas").TryGetProperty("ProblemDetails", out _).ShouldBeTrue();

        foreach (var path in root.GetProperty("paths").EnumerateObject())
        foreach (var operation in path.Value.EnumerateObject().Where(x => MetodosHttp.Contains(x.Name)))
        {
            var tags = operation.Value.GetProperty("tags");
            tags.GetArrayLength().ShouldBe(1, $"{operation.Name.ToUpperInvariant()} {path.Name} deve possuir uma única tag de módulo");
            operation.Value.GetProperty("summary").GetString().ShouldNotBeNullOrWhiteSpace();
            operation.Value.GetProperty("description").GetString().ShouldNotBeNullOrWhiteSpace();
        }
    }

    private static HashSet<string> LerOperacoesYaml(string[] linhas)
    {
        var resultado = new HashSet<string>(StringComparer.Ordinal);
        string? path = null;
        foreach (var linha in linhas)
        {
            if (linha.StartsWith("  ", StringComparison.Ordinal) && !linha.StartsWith("    ", StringComparison.Ordinal))
            {
                var chave = linha.Trim().TrimEnd(':').Trim('\'', '"');
                path = chave.StartsWith("/api/v1/", StringComparison.Ordinal) ? chave : null;
            }
            else if (path is not null && linha.StartsWith("    ", StringComparison.Ordinal) && !linha.StartsWith("      ", StringComparison.Ordinal))
            {
                var metodo = linha.Trim().TrimEnd(':');
                if (MetodosHttp.Contains(metodo)) resultado.Add($"{metodo.ToUpperInvariant()} {path}");
            }
        }
        return resultado;
    }

    private static readonly HashSet<string> MetodosHttp = ["get", "post", "put", "patch", "delete"];
}
