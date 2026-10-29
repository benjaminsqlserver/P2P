namespace P2P.Contracts.Audit;

/// <summary>Row shape for the audit trail screen.</summary>
public sealed record AuditEntryDto
{
    public required Guid Id { get; init; }
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>
    /// The acting user. The application keeps no user directory, so this is
    /// the identity provider's user identifier.
    /// </summary>
    public required string UserName { get; init; }

    public required string TableName { get; init; }
    public required string EntityId { get; init; }

    /// <summary>"Added", "Modified" or "Deleted".</summary>
    public required string Action { get; init; }

    public string? ChangedColumns { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
}
