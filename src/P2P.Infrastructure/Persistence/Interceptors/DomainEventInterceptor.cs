using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;

namespace P2P.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Collects domain events from every tracked aggregate and publishes them
/// AFTER the transaction commits, so a handler cannot observe state that
/// was subsequently rolled back.
/// </summary>
public sealed class DomainEventInterceptor(IDomainEventPublisher publisher)
    : SaveChangesInterceptor
{
    private readonly List<IDomainEvent> _pending = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var roots = eventData.Context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .ToArray();

        foreach (var root in roots)
        {
            _pending.AddRange(root.Entity.DomainEvents);
            root.Entity.ClearDomainEvents();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pending.Count > 0)
        {
            var toPublish = _pending.ToArray();
            _pending.Clear();

            foreach (var domainEvent in toPublish)
            {
                await publisher.PublishAsync(domainEvent, cancellationToken);
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
