using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Payments;
using P2P.Domain.Common;
using P2P.Domain.Invoices;

namespace P2P.Application.Payments.Queries.GetPayableInvoices;

/// <summary>Approved invoices due on or before the horizon and not yet on a payment.</summary>
public sealed record GetPayableInvoicesQuery(DateOnly DueOnOrBefore)
    : IQuery<IReadOnlyList<PayableInvoiceDto>>;

internal sealed class GetPayableInvoicesQueryHandler(
    IApplicationDbContext context,
    IClock clock)
    : IQueryHandler<GetPayableInvoicesQuery, IReadOnlyList<PayableInvoiceDto>>
{
    public async Task<Result<IReadOnlyList<PayableInvoiceDto>>> Handle(
        GetPayableInvoicesQuery request, CancellationToken ct)
    {
        var alreadyAllocated = PaymentQueries.InvoicesOnLivePayments(context);

        // Loaded as aggregates so the outstanding amount comes from the
        // domain's own Money arithmetic rather than a SQL approximation.
        var invoices = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => (i.Status == InvoiceStatus.Approved
                         || i.Status == InvoiceStatus.PartiallyPaid)
                        && i.DueDate <= request.DueOnOrBefore
                        && !alreadyAllocated.Contains(i.Id))
            .OrderBy(i => i.DueDate)
            .ToListAsync(ct);

        var vendorIds = invoices.Select(i => i.VendorId).Distinct().ToArray();

        var vendorNames = await context.Vendors
            .AsNoTracking()
            .Where(v => vendorIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.LegalName, ct);

        var today = clock.Today;

        IReadOnlyList<PayableInvoiceDto> payables = invoices
            .Select(i => new PayableInvoiceDto
            {
                InvoiceId = i.Id,
                VendorId = i.VendorId,
                VendorName = vendorNames.GetValueOrDefault(i.VendorId, "—"),
                InternalNumber = i.InternalNumber,
                VendorInvoiceNumber = i.VendorInvoiceNumber,
                DueDate = i.DueDate,
                DaysUntilDue = i.DueDate.DayNumber - today.DayNumber,
                AmountOutstanding = i.AmountOutstanding.Amount,
                Currency = i.Currency
            })
            .ToList();

        return Result.Success(payables);
    }
}
