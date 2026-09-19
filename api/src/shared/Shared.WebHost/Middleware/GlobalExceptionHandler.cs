using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Http.Results;

namespace Shared.WebHost.Middleware;

/// <summary>Exceções não tratadas viram ProblemDetails 500 sem vazar stack trace (fora de Development); o traceId permite achar o log.</summary>
internal sealed class GlobalExceptionHandler(IHostEnvironment environment, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        logger.LogError(exception, "Exceção não tratada. TraceId={TraceId}", traceId);

        var status = exception is OperationCanceledException ? StatusCodes.Status499ClientClosedRequest : StatusCodes.Status500InternalServerError;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = "Erro interno",
            Type = ResultHttpExtensions.ProblemTypeBase + "ErroInterno",
            Detail = environment.IsDevelopment() ? exception.Message : "Ocorreu um erro inesperado. Informe o traceId ao suporte.",
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["codigo"] = "ErroInterno";
        problem.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken: cancellationToken);
        return true;
    }
}
