namespace P2P.Domain.Common;

/// <summary>
/// The entry point to a consistency boundary. Only aggregate roots are
/// loaded and saved; everything inside an aggregate is reached through
/// its root and is saved with it in a single transaction.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(Guid id) : base(id) { }

    protected AggregateRoot() { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
