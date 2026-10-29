using P2P.Application.Abstractions.Messaging;
using P2P.Application.Dashboard.Queries.GetDashboardSummary;
using P2P.Application.Dashboard.Queries.GetInvoiceAgeing;
using P2P.Application.Dashboard.Queries.GetMonthlySpend;
using P2P.Contracts.Dashboard;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IDashboardApi"/>.</summary>
internal sealed class ServerDashboardApi(IDispatcher dispatcher) : IDashboardApi
{
    public async Task<DashboardSummaryDto?> GetSummaryAsync(CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetDashboardSummaryQuery(), ct);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<IReadOnlyList<MonthlySpendDto>> GetMonthlySpendAsync(
        int months, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetMonthlySpendQuery(months), ct);
        return result.IsSuccess ? result.Value : [];
    }

    public async Task<IReadOnlyList<AgeingBucketDto>> GetInvoiceAgeingAsync(
        CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetInvoiceAgeingQuery(), ct);
        return result.IsSuccess ? result.Value : [];
    }
}
