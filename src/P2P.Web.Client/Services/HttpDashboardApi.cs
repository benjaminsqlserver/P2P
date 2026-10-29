using System.Net.Http.Json;
using P2P.Contracts.Dashboard;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IDashboardApi"/>.</summary>
internal sealed class HttpDashboardApi(HttpClient http) : IDashboardApi
{
    public Task<DashboardSummaryDto?> GetSummaryAsync(CancellationToken ct = default) =>
        http.GetFromJsonAsync<DashboardSummaryDto>("api/dashboard/summary", ct);

    public async Task<IReadOnlyList<MonthlySpendDto>> GetMonthlySpendAsync(
        int months, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<MonthlySpendDto>>(
            $"api/dashboard/monthly-spend?months={months}", ct) ?? [];

    public async Task<IReadOnlyList<AgeingBucketDto>> GetInvoiceAgeingAsync(
        CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<AgeingBucketDto>>("api/dashboard/ageing", ct) ?? [];
}
