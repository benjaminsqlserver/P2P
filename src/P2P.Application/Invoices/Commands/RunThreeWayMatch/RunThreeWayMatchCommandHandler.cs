using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Invoices;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.Invoices.Commands.RunThreeWayMatch;

internal sealed class RunThreeWayMatchCommandHandler(
    IApplicationDbContext context,
    IClock clock)
    : ICommandHandler<RunThreeWayMatchCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        RunThreeWayMatchCommand command, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .Include(i => i.Lines)
            .Include(i => i.MatchExceptions)
            .SingleOrDefaultAsync(i => i.Id == command.InvoiceId, ct);

        if (invoice is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(Invoice), command.InvoiceId));
        }

        var purchaseOrder = await context.PurchaseOrders
            .Include(p => p.Lines)
            .SingleOrDefaultAsync(p => p.Id == invoice.PurchaseOrderId, ct);

        if (purchaseOrder is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(PurchaseOrder), invoice.PurchaseOrderId));
        }

        var duplicate = await context.Invoices
            .AsNoTracking()
            .AnyAsync(i => i.Id != invoice.Id
                           && i.VendorId == invoice.VendorId
                           && i.VendorInvoiceNumber == invoice.VendorInvoiceNumber,
                      ct);

        var tolerance = MatchTolerance.Default(invoice.Currency);

        var matchResult = ThreeWayMatchService.Match(
            invoice, purchaseOrder, tolerance, clock.UtcNow, duplicate);

        if (matchResult.IsFailure)
        {
            return Result.Failure<Unit>(matchResult.Error);
        }

        // When the match succeeds, consume the received quantities so a
        // second invoice cannot bill them again.
        if (invoice.Status == InvoiceStatus.Matched)
        {
            var applied = purchaseOrder.ApplyInvoiceMatch(
                invoice.InvoicedQuantitiesByPoLine());

            if (applied.IsFailure)
            {
                return Result.Failure<Unit>(applied.Error);
            }
        }

        return Result.Success(Unit.Value);
    }
}
