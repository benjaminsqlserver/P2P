using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;

namespace P2P.Infrastructure.Services;

/// <summary>
/// In-process publisher: resolves every <see cref="IDomainEventHandler{TEvent}"/>
/// registered for the event's runtime type and invokes them in turn.
/// Called by <see cref="Persistence.Interceptors.DomainEventInterceptor"/>
/// after the transaction commits.
/// </summary>
internal sealed class DomainEventPublisher(IServiceProvider provider) : IDomainEventPublisher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypes = new();

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var handlerType = HandlerTypes.GetOrAdd(
            domainEvent.GetType(),
            static eventType => typeof(IDomainEventHandler<>).MakeGenericType(eventType));

        foreach (var handler in provider.GetServices(handlerType))
        {
            if (handler is null)
            {
                continue;
            }

            var task = (Task)handlerType
                .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!
                .Invoke(handler, [domainEvent, ct])!;

            await task;
        }
    }
}
