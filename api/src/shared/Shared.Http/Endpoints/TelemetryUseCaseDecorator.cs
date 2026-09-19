using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Http.Results;
using Shared.Observability.Telemetria;

namespace Shared.Http.Endpoints;

/// <summary>
/// Decorator transparente aplicado a TODO caso de uso: span de trace, duração, contadores de execução/falha e log
/// estruturado do erro de negócio. Os casos de uso ficam livres de código de observabilidade.
/// </summary>
internal sealed class TelemetryUseCaseDecorator<TRequest, TResponse>(
    IUseCase<TRequest, TResponse> inner,
    ModuleTelemetry telemetry,
    ILogger<TelemetryUseCaseDecorator<TRequest, TResponse>> logger) : IUseCase<TRequest, TResponse>
{
    private static readonly string UseCaseName = ResolverNome();

    private static string ResolverNome()
    {
        var nome = typeof(TRequest).Name;
        return nome.EndsWith("Request", StringComparison.Ordinal) ? nome[..^"Request".Length] : nome;
    }

    public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        using var activity = telemetry.StartActivity(UseCaseName);
        var inicio = Stopwatch.GetTimestamp();
        try
        {
            var resultado = await inner.HandleAsync(request, cancellationToken);
            var duracao = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            if (resultado.IsFailure)
            {
                activity?.SetTag("error.code", resultado.Error.Code);
                activity?.SetTag("error.type", resultado.Error.Type.ToString());
                logger.LogInformation("Caso de uso {UseCase} retornou {ErrorCode}: {ErrorMessage}", UseCaseName, resultado.Error.Code, resultado.Error.Message);
            }

            telemetry.RegistrarExecucao(UseCaseName, resultado.IsSuccess, duracao, resultado.IsFailure ? resultado.Error.Code : null);
            return resultado;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            telemetry.RegistrarExecucao(UseCaseName, false, Stopwatch.GetElapsedTime(inicio).TotalMilliseconds, "Excecao");
            throw;
        }
    }
}
