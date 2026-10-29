using P2P.Application.Abstractions;
using P2P.Domain.Payments;

namespace P2P.Application.Payments;

internal static class PaymentQueries
{
    /// <summary>
    /// Invoices already allocated on a payment that is still live. Excluding
    /// them is what stops two payment runs paying the same invoice.
    /// </summary>
    public static IQueryable<Guid> InvoicesOnLivePayments(IApplicationDbContext context) =>
        from a in context.PaymentAllocations
        join p in context.Payments on a.PaymentId equals p.Id
        where p.Status == PaymentStatus.Draft
              || p.Status == PaymentStatus.Authorised
              || p.Status == PaymentStatus.Executed
        select a.InvoiceId;
}
