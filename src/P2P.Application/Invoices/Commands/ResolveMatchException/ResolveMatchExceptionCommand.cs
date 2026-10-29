using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Invoices;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.Invoices.Commands.ResolveMatchException;

public sealed record ResolveMatchExceptionCommand(
    Guid InvoiceId, Guid ExceptionId, string Resolution) : ICommand;

internal sealed class ResolveMatchExceptionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : ICommandHandler<ResolveMatchExceptionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        ResolveMatchExceptionCommand command, CancellationToken ct)
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

        // Exceptions are only cleared on an invoice that is in exception;
        // anything else (disputed, approved, paid) must not drift back to
        // Matched and become approvable again.
        if (invoice.Status != InvoiceStatus.Exception)
        {
            return Result.Failure<Unit>(InvoiceErrors.NotEditable);
        }

        var result = invoice.ResolveException(
            command.ExceptionId, currentUser.UserId, command.Resolution, clock.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<Unit>(result.Error);
        }

        // Clearing the last exception matches the invoice. Consume the PO
        // quantities exactly as RunThreeWayMatch does for a clean match, so
        // a second invoice cannot bill the same received goods.
        if (invoice.Status == InvoiceStatus.Matched)
        {
            var purchaseOrder = await context.PurchaseOrders
                .Include(p => p.Lines)
                .SingleOrDefaultAsync(p => p.Id == invoice.PurchaseOrderId, ct);

            if (purchaseOrder is null)
            {
                return Result.Failure<Unit>(
                    Error.NotFound(nameof(PurchaseOrder), invoice.PurchaseOrderId));
            }

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
