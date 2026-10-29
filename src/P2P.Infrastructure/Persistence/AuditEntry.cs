namespace P2P.Infrastructure.Persistence;

/// <summary>
/// An immutable record of a change to a tracked entity. Written by
/// <see cref="Interceptors.AuditInterceptor"/> inside the same transaction
/// as the change itself, so an audit row can never be missing.
/// </summary>
public sealed class AuditEntry
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string TableName { get; set; }

    public required string EntityId { get; set; }

    public required string Action { get; set; }

    public required Guid UserId { get; set; }

    public required DateTimeOffset OccurredAtUtc { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? ChangedColumns { get; set; }
}
