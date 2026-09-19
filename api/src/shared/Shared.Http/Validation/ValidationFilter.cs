using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Http.Results;

namespace Shared.Http.Validation;

/// <summary>Filtro de endpoint: valida o request com FluentValidation e devolve 400 ValidationProblemDetails antes de chegar ao caso de uso.</summary>
public sealed class ValidationFilter<TRequest>(ILogger<ValidationFilter<TRequest>> logger) : IEndpointFilter where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (validator is null || request is null)
        {
            logger.LogDebug(
                "Validação não executada para {RequestType}: validador presente={ValidatorPresent}, request presente={RequestPresent}",
                typeof(TRequest).Name, validator is not null, request is not null);
            return await next(context);
        }

        PreencherIdentificadoresDaRota(request, context.HttpContext.Request.RouteValues);

        var resultado = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (resultado.IsValid)
        {
            logger.LogDebug("Validação de {RequestType} concluída sem violações", typeof(TRequest).Name);
            return await next(context);
        }

        var erros = resultado.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        logger.LogWarning(
            "Validação de {RequestType} rejeitou a requisição com {ValidationErrorCount} violação(ões) nos campos {ValidationFields}",
            typeof(TRequest).Name, resultado.Errors.Count, erros.Keys.Order(StringComparer.Ordinal).ToArray());

        return TypedResults.ValidationProblem(erros,
            title: "Requisição inválida",
            type: ResultHttpExtensions.ProblemTypeBase + "Validacao",
            extensions: new Dictionary<string, object?> { ["codigo"] = "Validacao" });
    }

    /// <summary>
    /// O filtro roda antes do delegate do endpoint. Portanto, propriedades técnicas como <c>PalestraId</c>,
    /// marcadas com JsonIgnore e preenchidas pelo parâmetro de rota, precisam ser materializadas antes da validação.
    /// A rota <c>{id}</c> representa o primeiro identificador vazio; rotas nomeadas, como <c>{salaId}</c>,
    /// são associadas pelo nome da propriedade.
    /// </summary>
    private static void PreencherIdentificadoresDaRota(TRequest request, RouteValueDictionary routeValues)
    {
        var propriedades = typeof(TRequest).GetProperties()
            .Where(p => p.CanWrite && p.PropertyType == typeof(Guid) && p.Name.EndsWith("Id", StringComparison.Ordinal))
            .ToList();

        foreach (var (chave, valor) in routeValues)
        {
            if (!Guid.TryParse(Convert.ToString(valor, System.Globalization.CultureInfo.InvariantCulture), out var id))
            {
                continue;
            }

            var propriedade = propriedades.FirstOrDefault(p => string.Equals(p.Name, chave, StringComparison.OrdinalIgnoreCase));
            if (propriedade is null && string.Equals(chave, "id", StringComparison.OrdinalIgnoreCase))
            {
                propriedade = propriedades.FirstOrDefault(p => (Guid)(p.GetValue(request) ?? Guid.Empty) == Guid.Empty);
            }

            propriedade?.SetValue(request, id);
        }
    }
}

public static class ValidationFilterExtensions
{
    /// <summary>Aplica validação FluentValidation ao request do endpoint e documenta a resposta 400.</summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) where TRequest : class =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
}
