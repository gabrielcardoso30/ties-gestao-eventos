using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Shared.Observability.Middleware;

/// <summary>Garante um X-Correlation-Id por requisição (aceita o do cliente), devolve no response e enriquece logs e trace.</summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var valor) && !string.IsNullOrWhiteSpace(valor)
            ? valor.ToString()
            : Activity.Current?.TraceId.ToString() ?? Guid.CreateVersion7().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });
        Activity.Current?.SetTag("correlation.id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
