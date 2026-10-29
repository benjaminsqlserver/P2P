namespace P2P.Domain.Common;

/// <summary>
/// Something that happened in the domain which other parts of the system
/// may care about. Domain events are named in the past tense.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}

/// <summary>
/// Convenience base record supplying identity and timestamp.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
