using FluentValidation;
using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.GoodsReceipts;
using P2P.Domain.Common;
using P2P.Domain.GoodsReceipts;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.GoodsReceipts.Commands.CreateGoodsReceipt;

public sealed record CreateGoodsReceiptCommand(CreateGoodsReceiptRequest Request)
    : ICommand<Guid>;

internal sealed class CreateGoodsReceiptCommandValidator
    : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptCommandValidator()
    {
        RuleFor(c => c.Request.PurchaseOrderId).NotEmpty();
        RuleFor(c => c.Request.DeliveryNoteReference).MaximumLength(100);
        RuleFor(c => c.Request.Carrier).MaximumLength(100);

        RuleFor(c => c.Request.Lines)
            .NotEmpty()
            .WithMessage("A goods receipt must record at least one line.");

        RuleForEach(c => c.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.PurchaseOrderLineId).NotEmpty();
            line.RuleFor(l => l.QuantityAccepted).GreaterThanOrEqualTo(0m);
            line.RuleFor(l => l.QuantityRejected).GreaterThanOrEqualTo(0m);
            line.RuleFor(l => l.RejectionReason).MaximumLength(1000);
            line.RuleFor(l => l.Notes).MaximumLength(1000);
        });
    }
}

/// <summary>
/// Records the receipt document and applies its accepted quantities to the
/// purchase order in one transaction, so a receipt can never exist without
/// the order reflecting it (or vice versa).
/// </summary>
internal sealed class CreateGoodsReceiptCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock,
    INumberSequence numbers)
    : ICommandHandler<CreateGoodsReceiptCommand, Guid>
{
    // The book specifies an over-receipt tolerance (invariant 11) but no
    // value; none is permitted until it is made configurable.
    private const decimal OverReceiptTolerancePercent = 0m;

    public async Task<Result<Guid>> Handle(
        CreateGoodsReceiptCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var order = await context.PurchaseOrders
            .Include(p => p.Lines)
            .SingleOrDefaultAsync(p => p.Id == request.PurchaseOrderId, ct);

        if (order is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(nameof(PurchaseOrder), request.PurchaseOrderId));
        }

        if (!order.IsReceivable)
        {
            return Result.Failure<Guid>(PurchaseOrderErrors.NotReceivable);
        }

        var number = await numbers.NextAsync("GRN", ct);

        var receiptResult = GoodsReceipt.Create(
            number,
            order.Id,
            currentUser.UserId,
            request.ReceivedOn,
            request.DeliveryNoteReference,
            request.Carrier,
            clock.Today);

        if (receiptResult.IsFailure)
        {
            return Result.Failure<Guid>(receiptResult.Error);
        }

        var receipt = receiptResult.Value;

        foreach (var line in request.Lines)
        {
            var added = receipt.AddLine(
                line.PurchaseOrderLineId,
                line.QuantityAccepted,
                line.QuantityRejected,
                line.RejectionReason,
                line.Notes);

            if (added.IsFailure)
            {
                return Result.Failure<Guid>(added.Error);
            }
        }

        var confirmed = receipt.Confirm();

        if (confirmed.IsFailure)
        {
            return Result.Failure<Guid>(confirmed.Error);
        }

        var accepted = receipt.AcceptedQuantitiesByPoLine();

        // A delivery that was rejected in full changes nothing on the order.
        if (accepted.Count > 0)
        {
            var applied = order.ApplyReceipt(accepted, OverReceiptTolerancePercent);

            if (applied.IsFailure)
            {
                return Result.Failure<Guid>(applied.Error);
            }
        }

        context.GoodsReceipts.Add(receipt);

        return receipt.Id;
    }
}
