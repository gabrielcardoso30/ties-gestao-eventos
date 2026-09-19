using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shared.Http.Results;

/// <summary>Converte Result em respostas HTTP com ProblemDetails (RFC 9457) nas falhas.</summary>
public static class ResultHttpExtensions
{
    public const string ProblemTypeBase = "https://gestao-eventos.globalsys.com.br/erros/";

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> location) =>
        result.IsSuccess ? TypedResults.Created(location(result.Value), result.Value) : result.Error.ToProblem();

    public static IResult ToNoContentResult(this Result result) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();

    public static IResult ToNoContentResult<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();

    public static IResult ToProblem(this Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = Titulo(error.Type),
            Detail = error.Message,
            Type = ProblemTypeBase + error.Code,
        };
        problem.Extensions["codigo"] = error.Code;
        return TypedResults.Problem(problem);
    }

    private static string Titulo(ErrorType type) => type switch
    {
        ErrorType.Validation => "Requisição inválida",
        ErrorType.NotFound => "Recurso não encontrado",
        ErrorType.Conflict => "Conflito",
        ErrorType.BusinessRule => "Regra de negócio violada",
        ErrorType.Unauthorized => "Não autenticado",
        ErrorType.Forbidden => "Acesso negado",
        _ => "Erro interno",
    };
}
