using Shared.Http.Results;

namespace Shared.Http.Endpoints;

/// <summary>Caso de uso com entrada e saída. Registrado automaticamente por varredura de assembly (scoped).</summary>
public interface IUseCase<in TRequest, TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
