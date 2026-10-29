using P2P.Domain.Common;

namespace P2P.Application.Abstractions.Messaging;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken ct);
}
