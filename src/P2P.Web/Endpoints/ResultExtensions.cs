using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Common;
using P2P.Domain.Common;

namespace P2P.Web.Endpoints;

public static class ResultExtensions
{
    public static OperationResult ToOperationResult(this Result result) =>
        result.IsSuccess
            ? OperationResult.Success()
            : OperationResult.Failure(result.Error.Code, result.Error.Description);

    public static OperationResult<T> ToOperationResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? OperationResult<T>.Success(result.Value)
            : OperationResult<T>.Failure(
                result.Error.Code, result.Error.Description);

    public static OperationResult ToOperationResult(this Result<Unit> result) =>
        result.IsSuccess
            ? OperationResult.Success()
            : OperationResult.Failure(result.Error.Code, result.Error.Description);

    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess
            ? Results.Ok(OperationResult.Success())
            : Problem(result.Error);

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : Problem(result.Error);

    private static IResult Problem(Error error)
    {
        var statusCode = StatusCodeFor(error.Code);

        return Results.Problem(
            title: error.Code,
            detail: error.Description,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code
            });
    }

    private static int StatusCodeFor(string errorCode) => errorCode switch
    {
        var c when c.EndsWith(".NotFound", StringComparison.Ordinal)
            => StatusCodes.Status404NotFound,

        var c when c.Contains("Forbidden", StringComparison.Ordinal)
            => StatusCodes.Status403Forbidden,

        var c when c.Contains("AlreadyExists", StringComparison.Ordinal)
                || c.Contains("Duplicate", StringComparison.Ordinal)
            => StatusCodes.Status409Conflict,

        _ => StatusCodes.Status400BadRequest
    };
}
