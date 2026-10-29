using System.Diagnostics;
using Microsoft.Extensions.Logging;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;

namespace P2P.Application.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "Handling {RequestName} for user {UserId}", name, currentUser.UserId);

        var result = await next();

        stopwatch.Stop();

        if (result.IsSuccess)
        {
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds} ms",
                name, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogWarning(
                "{RequestName} failed with {ErrorCode}: {ErrorDescription} "
              + "after {ElapsedMilliseconds} ms",
                name, result.Error.Code, result.Error.Description,
                stopwatch.ElapsedMilliseconds);
        }

        return result;
    }
}
