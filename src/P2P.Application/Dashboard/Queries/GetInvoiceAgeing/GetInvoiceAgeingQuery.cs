using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Dashboard;
using P2P.Domain.Common;
using P2P.Domain.Invoices;

namespace P2P.Application.Dashboard.Queries.GetInvoiceAgeing;

/// <summary>Outstanding amounts on unpaid invoices, bucketed by days past due.</summary>
public sealed record GetInvoiceAgeingQuery : IQuery<IReadOnlyList<AgeingBucketDto>>;

internal sealed class GetInvoiceAgeingQueryHandler(
    IApplicationDbContext context,
    IClock clock)
    : IQueryHandler<GetInvoiceAgeingQuery, IReadOnlyList<AgeingBucketDto>>
{
    private static readonly (string Label, int MaxDaysOverdue)[] Buckets =
    [
        ("Not yet due", 0),
        ("1–30 days", 30),
        ("31–60 days", 60),
        ("61–90 days", 90),
        ("Over 90 days", int.MaxValue)
    ];

    public async Task<Result<IReadOnlyList<AgeingBucketDto>>> Handle(
        GetInvoiceAgeingQuery request, CancellationToken ct)
    {
        // Loaded as aggregates so the outstanding amount comes from the
        // domain's Money arithmetic, as the payment proposal does.
        var invoices = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => i.Status != InvoiceStatus.Paid
                        && i.Status != InvoiceStatus.Cancelled
                        && i.Currency == DashboardCurrency.Code)
            .ToListAsync(ct);

        var today = clock.Today;

        var totals = invoices
            .GroupBy(i => BucketFor(today.DayNumber - i.DueDate.DayNumber))
            .ToDictionary(g => g.Key, g => g.Sum(i => i.AmountOutstanding.Amount));

        IReadOnlyList<AgeingBucketDto> result = Buckets
            .Select(b => new AgeingBucketDto
            {
                Bucket = b.Label,
                Amount = totals.GetValueOrDefault(b.Label)
            })
            .ToList();

        return Result.Success(result);
    }

    private static string BucketFor(int daysOverdue) =>
        Buckets.First(b => daysOverdue <= b.MaxDaysOverdue).Label;
}
