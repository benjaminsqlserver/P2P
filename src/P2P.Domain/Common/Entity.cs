namespace P2P.Domain.Common;

public abstract class Entity : IEquatable<Entity>
{
    protected Entity(Guid id) => Id = id;

    /// <summary>Required by EF Core's materialiser. Not for application use.</summary>
    protected Entity() { }

    public Guid Id { get; protected set; }

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other.GetType() == GetType() && other.Id == Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) =>
        !Equals(left, right);
}
