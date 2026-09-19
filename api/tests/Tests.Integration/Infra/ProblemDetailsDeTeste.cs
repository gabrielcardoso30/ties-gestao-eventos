using System.Text.Json;

namespace Tests.Integration.Infra;

/// <summary>Leitura mínima do ProblemDetails retornado pela API (RFC 9457 + extensão "codigo").</summary>
public sealed record ProblemDetailsDeTeste(int? Status, string? Title, string? Detail, string? Codigo, string? TraceId, Dictionary<string, string[]>? Errors)
{
    public static async Task<ProblemDetailsDeTeste> LerAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Dictionary<string, string[]>? errors = null;
        if (root.TryGetProperty("errors", out var e))
        {
            errors = e.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.EnumerateArray().Select(v => v.GetString()!).ToArray());
        }

        return new ProblemDetailsDeTeste(
            root.TryGetProperty("status", out var s) ? s.GetInt32() : null,
            root.TryGetProperty("title", out var t) ? t.GetString() : null,
            root.TryGetProperty("detail", out var d) ? d.GetString() : null,
            root.TryGetProperty("codigo", out var c) ? c.GetString() : null,
            root.TryGetProperty("traceId", out var tr) ? tr.GetString() : null,
            errors);
    }
}
