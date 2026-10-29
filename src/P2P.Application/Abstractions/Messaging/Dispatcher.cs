using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using P2P.Domain.Common;

namespace P2P.Application.Abstractions.Messaging;

internal sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<Result<TResponse>> Send<TResponse>(
        ICommand<TResponse> command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var wrapper = (CommandWrapperBase<TResponse>)Wrappers.GetOrAdd(
            command.GetType(),
            static commandType =>
            {
                var wrapperType = typeof(CommandWrapper<,>)
                    .MakeGenericType(commandType, typeof(TResponse));

                return Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException(
                        $"Could not construct a dispatcher wrapper for "
                      + $"{commandType.Name}.");
            });

        return wrapper.Handle(command, provider, ct);
    }

    public Task<Result<TResponse>> Query<TResponse>(
        IQuery<TResponse> query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var wrapper = (QueryWrapperBase<TResponse>)Wrappers.GetOrAdd(
            query.GetType(),
            static queryType =>
            {
                var wrapperType = typeof(QueryWrapper<,>)
                    .MakeGenericType(queryType, typeof(TResponse));

                return Activator.CreateInstance(wrapperType)
                    ?? throw new InvalidOperationException(
                        $"Could not construct a dispatcher wrapper for "
                      + $"{queryType.Name}.");
            });

        return wrapper.Handle(query, provider, ct);
    }

    private abstract class CommandWrapperBase<TResponse>
    {
        public abstract Task<Result<TResponse>> Handle(
            object command, IServiceProvider sp, CancellationToken ct);
    }

    private sealed class CommandWrapper<TCommand, TResponse>
        : CommandWrapperBase<TResponse>
        where TCommand : ICommand<TResponse>
    {
        public override Task<Result<TResponse>> Handle(
            object command, IServiceProvider sp, CancellationToken ct)
        {
            var handler = sp.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
            var typed = (TCommand)command;

            RequestHandlerDelegate<TResponse> pipeline =
                () => handler.Handle(typed, ct);

            var behaviors = sp
                .GetServices<IPipelineBehavior<TCommand, TResponse>>()
                .Reverse();

            foreach (var behavior in behaviors)
            {
                var next = pipeline;
                var current = behavior;
                pipeline = () => current.Handle(typed, next, ct);
            }

            return pipeline();
        }
    }

    private abstract class QueryWrapperBase<TResponse>
    {
        public abstract Task<Result<TResponse>> Handle(
            object query, IServiceProvider sp, CancellationToken ct);
    }

    private sealed class QueryWrapper<TQuery, TResponse>
        : QueryWrapperBase<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public override Task<Result<TResponse>> Handle(
            object query, IServiceProvider sp, CancellationToken ct)
        {
            var handler = sp.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
            var typed = (TQuery)query;

            RequestHandlerDelegate<TResponse> pipeline =
                () => handler.Handle(typed, ct);

            var behaviors = sp
                .GetServices<IPipelineBehavior<TQuery, TResponse>>()
                .Reverse();

            foreach (var behavior in behaviors)
            {
                var next = pipeline;
                var current = behavior;
                pipeline = () => current.Handle(typed, next, ct);
            }

            return pipeline();
        }
    }
}
