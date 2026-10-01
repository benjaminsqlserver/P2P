using P2P.Domain.Common;

namespace P2P.Domain.Invoices;

public sealed class MatchException : Entity
{
    private MatchException(
        Guid id,
        Guid invoiceId,
        Guid? invoiceLineId,
        MatchExceptionType type,
        string description,
        decimal? expectedValue,
        decimal? actualValue)
        : base(id)
    {
        InvoiceId = invoiceId;
        InvoiceLineId = invoiceLineId;
        Type = type;
        Description = description;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
        IsResolved = false;
    }

    private MatchException() { }

    public Guid InvoiceId { get; private set; }

    public Guid? InvoiceLineId { get; private set; }

    public MatchExceptionType Type { get; private set; }

    public string Description { get; private set; } = null!;

    public decimal? ExpectedValue { get; private set; }

    public decimal? ActualValue { get; private set; }

    public decimal? Variance =>
        ExpectedValue.HasValue && ActualValue.HasValue
            ? ActualValue.Value - ExpectedValue.Value
            : null;

    public bool IsResolved { get; private set; }

    public Guid? ResolvedByUserId { get; private set; }

    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    public string? Resolution { get; private set; }

    internal static MatchException Create(
        Guid invoiceId,
        Guid? invoiceLineId,
        MatchExceptionType type,
        string description,
        decimal? expectedValue = null,
        decimal? actualValue = null) =>
        new(Guid.CreateVersion7(), invoiceId, invoiceLineId, type,
            description, expectedValue, actualValue);

    internal void Resolve(Guid userId, string resolution, DateTimeOffset nowUtc)
    {
        IsResolved = true;
        ResolvedByUserId = userId;
        ResolvedAtUtc = nowUtc;
        Resolution = resolution;
    }
}
