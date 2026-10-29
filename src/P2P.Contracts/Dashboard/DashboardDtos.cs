namespace P2P.Contracts.Dashboard;

/// <summary>Headline figures for the dashboard metric cards.</summary>
public sealed record DashboardSummaryDto
{
    public required int AwaitingMyApproval { get; init; }
    public required int OpenPurchaseOrders { get; init; }
    public required int UnresolvedExceptions { get; init; }
    public required decimal PayableThisWeek { get; init; }
}

public sealed record MonthlySpendDto
{
    /// <summary>Display label, e.g. "Oct".</summary>
    public required string Month { get; init; }
    public required decimal Amount { get; init; }
}

public sealed record AgeingBucketDto
{
    public required string Bucket { get; init; }
    public required decimal Amount { get; init; }
}
