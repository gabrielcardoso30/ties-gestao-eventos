using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Http.Results;
using Shared.Observability.Telemetria;

namespace Shared.Http.Endpoints;

/// <summary>
/// Decorator transparente aplicado a TODO caso de uso: garante o envelope comum de início/fim, span de trace,
/// duração, contadores de execução/falha e erro estruturado; os logs internos detalham o fluxo de negócio.
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
        var contexto = UseCaseLogContext.From(request);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["Modulo"] = telemetry.Modulo,
            ["UseCase"] = UseCaseName
        });

        activity?.SetTag("usecase.name", UseCaseName);
        activity?.SetTag("module.name", telemetry.Modulo);
        foreach (var (chave, valor) in contexto)
        {
            activity?.SetTag($"request.{chave}", valor);
        }

        logger.LogInformation("Iniciando caso de uso {UseCase} com {@RequestContext}", UseCaseName, contexto);
        try
        {
            var resultado = await inner.HandleAsync(request, cancellationToken);
            var duracao = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            if (resultado.IsFailure)
            {
                activity?.SetTag("error.code", resultado.Error.Code);
                activity?.SetTag("error.type", resultado.Error.Type.ToString());
                logger.LogWarning(
                    "Caso de uso {UseCase} rejeitado pela regra {ErrorCode} ({ErrorType}) em {DurationMs:0.0} ms: {ErrorMessage}",
                    UseCaseName, resultado.Error.Code, resultado.Error.Type, duracao, resultado.Error.Message);
            }
            else
            {
                logger.LogInformation("Caso de uso {UseCase} concluído com sucesso em {DurationMs:0.0} ms", UseCaseName, duracao);
            }

            telemetry.RegistrarExecucao(UseCaseName, resultado.IsSuccess, duracao, resultado.IsFailure ? resultado.Error.Code : null);
            return resultado;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var duracao = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            activity?.SetStatus(ActivityStatusCode.Error, "Cancelado");
            activity?.SetTag("canceled", true);
            telemetry.RegistrarExecucao(UseCaseName, false, duracao, "Cancelado");
            logger.LogWarning("Caso de uso {UseCase} cancelado após {DurationMs:0.0} ms", UseCaseName, duracao);
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var duracao = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            telemetry.RegistrarExecucao(UseCaseName, false, duracao, "Excecao");
            logger.LogError(ex, "Caso de uso {UseCase} falhou inesperadamente após {DurationMs:0.0} ms", UseCaseName, duracao);
            throw;
        }
    }
}
