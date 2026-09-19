using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shared.Http.Results;

namespace Shared.Http.Validation;

/// <summary>Filtro de endpoint: valida o request com FluentValidation e devolve 400 ValidationProblemDetails antes de chegar ao caso de uso.</summary>
public sealed class ValidationFilter<TRequest> : IEndpointFilter where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (validator is null || request is null)
        {
            return await next(context);
        }

        var resultado = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (resultado.IsValid)
        {
            return await next(context);
        }

        var erros = resultado.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return TypedResults.ValidationProblem(erros,
            title: "Requisição inválida",
            type: ResultHttpExtensions.ProblemTypeBase + "Validacao",
            extensions: new Dictionary<string, object?> { ["codigo"] = "Validacao" });
    }
}

public static class ValidationFilterExtensions
{
    /// <summary>Aplica validação FluentValidation ao request do endpoint e documenta a resposta 400.</summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) where TRequest : class =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
}
