using System.Globalization;
using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Dashboard;
using P2P.Domain.Common;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.Dashboard.Queries.GetMonthlySpend;

/// <summary>Committed spend: gross value of purchase orders by order month.</summary>
public sealed record GetMonthlySpendQuery(int Months) : IQuery<IReadOnlyList<MonthlySpendDto>>;

internal sealed class GetMonthlySpendQueryHandler(
    IApplicationDbContext context,
    IClock clock)
    : IQueryHandler<GetMonthlySpendQuery, IReadOnlyList<MonthlySpendDto>>
{
    // Labels are bare month names, which stay unique only within a year.
    private const int MaxMonths = 12;

    public async Task<Result<IReadOnlyList<MonthlySpendDto>>> Handle(
        GetMonthlySpendQuery request, CancellationToken ct)
    {
        var months = Math.Clamp(request.Months, 1, MaxMonths);
        var today = clock.Today;
        var from = new DateOnly(today.Year, today.Month, 1).AddMonths(-(months - 1));

        var orders = await context.PurchaseOrders
            .AsNoTracking()
            .Where(p => p.Status != PurchaseOrderStatus.Draft
                        && p.Status != PurchaseOrderStatus.Cancelled
                        && p.Currency == DashboardCurrency.Code
                        && p.OrderDate >= from)
            .Select(p => new
            {
                p.OrderDate,
                Amount = p.Lines.Sum(l =>
                    l.UnitPrice.Amount * l.QuantityOrdered * (1m + (l.TaxRatePercent / 100m)))
            })
            .ToListAsync(ct);

        var byMonth = orders
            .GroupBy(o => (o.OrderDate.Year, o.OrderDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.Amount));

        // Every month appears, including those with no orders, so the chart's
        // category axis is continuous.
        IReadOnlyList<MonthlySpendDto> result = Enumerable.Range(0, months)
            .Select(i => from.AddMonths(i))
            .Select(m => new MonthlySpendDto
            {
                Month = m.ToString("MMM", CultureInfo.InvariantCulture),
                Amount = decimal.Round(
                    byMonth.GetValueOrDefault((m.Year, m.Month)), 2, MidpointRounding.ToEven)
            })
            .ToList();

        return Result.Success(result);
    }
}
