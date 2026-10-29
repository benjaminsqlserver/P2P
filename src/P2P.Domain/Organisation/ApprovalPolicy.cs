using P2P.Domain.Common;

namespace P2P.Domain.Organisation;

/// <summary>
/// A rule stating that requisitions at or above <see cref="MinimumAmount"/>
/// require approval by <see cref="RequiredRole"/> at position
/// <see cref="Sequence"/> in the route. Rules may be scoped to a cost centre.
/// </summary>
public sealed class ApprovalPolicy : AggregateRoot
{
    private ApprovalPolicy(
        Guid id,
        Guid? costCentreId,
        string currency,
        decimal minimumAmount,
        int sequence,
        string requiredRole,
        bool isActive)
        : base(id)
    {
        CostCentreId = costCentreId;
        Currency = currency;
        MinimumAmount = minimumAmount;
        Sequence = sequence;
        RequiredRole = requiredRole;
        IsActive = isActive;
    }

    private ApprovalPolicy() { }

    public Guid? CostCentreId { get; private set; }

    public string Currency { get; private set; } = null!;

    public decimal MinimumAmount { get; private set; }

    public int Sequence { get; private set; }

    public string RequiredRole { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static ApprovalPolicy Create(
        Guid? costCentreId,
        string currency,
        decimal minimumAmount,
        int sequence,
        string requiredRole) =>
        new(Guid.CreateVersion7(), costCentreId, currency.ToUpperInvariant(),
            minimumAmount, sequence, requiredRole, isActive: true);

    public void Deactivate() => IsActive = false;
}
