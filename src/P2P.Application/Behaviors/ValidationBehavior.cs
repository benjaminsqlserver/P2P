using FluentValidation;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;

namespace P2P.Application.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator for the request and
/// short-circuits with a failed Result if any rule is broken. Handlers can
/// therefore assume their input is structurally sound and concern themselves
/// only with business rules that require state.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var applicable = validators.ToArray();

        if (applicable.Length == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                applicable.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        var first = failures[0];

        var error = Error.Validation(
            first.ErrorCode ?? $"{typeof(TRequest).Name}.Validation",
            string.Join(" ", failures.Select(f => f.ErrorMessage).Distinct()));

        return Result.Failure<TResponse>(error);
    }
}
