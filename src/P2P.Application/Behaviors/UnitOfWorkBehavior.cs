using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;

namespace P2P.Application.Behaviors;

/// <summary>
/// Commits the unit of work after a successful command. Queries are not
/// wrapped: this behaviour is registered only for command types.
/// </summary>
internal sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IApplicationDbContext context)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var result = await next();

        var isCommand = request.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType
                      && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (result.IsSuccess && isCommand)
        {
            await context.SaveChangesAsync(ct);
        }

        return result;
    }
}
