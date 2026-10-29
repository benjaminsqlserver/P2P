namespace P2P.Contracts.Dashboard;

public interface IDashboardApi
{
    Task<DashboardSummaryDto?> GetSummaryAsync(CancellationToken ct = default);

    /// <summary>Committed (purchase order) spend per month, oldest first; at most 12 months.</summary>
    Task<IReadOnlyList<MonthlySpendDto>> GetMonthlySpendAsync(
        int months, CancellationToken ct = default);

    /// <summary>Outstanding invoice amounts bucketed by days past due.</summary>
    Task<IReadOnlyList<AgeingBucketDto>> GetInvoiceAgeingAsync(
        CancellationToken ct = default);
}
