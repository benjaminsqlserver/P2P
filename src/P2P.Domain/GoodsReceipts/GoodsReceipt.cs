using P2P.Domain.Common;

namespace P2P.Domain.GoodsReceipts;

public sealed class GoodsReceipt : AggregateRoot
{
    private readonly List<GoodsReceiptLine> _lines = [];

    private GoodsReceipt(
        Guid id,
        string number,
        Guid purchaseOrderId,
        Guid receivedByUserId,
        DateOnly receivedOn,
        string? deliveryNoteReference,
        string? carrier)
        : base(id)
    {
        Number = number;
        PurchaseOrderId = purchaseOrderId;
        ReceivedByUserId = receivedByUserId;
        ReceivedOn = receivedOn;
        DeliveryNoteReference = deliveryNoteReference;
        Carrier = carrier;
    }

    private GoodsReceipt() { }

    public string Number { get; private set; } = null!;

    public Guid PurchaseOrderId { get; private set; }

    public Guid ReceivedByUserId { get; private set; }

    public DateOnly ReceivedOn { get; private set; }

    public string? DeliveryNoteReference { get; private set; }

    public string? Carrier { get; private set; }

    public bool IsReversed { get; private set; }

    public string? ReversalReason { get; private set; }

    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    public static Result<GoodsReceipt> Create(
        string number,
        Guid purchaseOrderId,
        Guid receivedByUserId,
        DateOnly receivedOn,
        string? deliveryNoteReference,
        string? carrier,
        DateOnly today)
    {
        if (receivedOn > today)
        {
            return Result.Failure<GoodsReceipt>(
                GoodsReceiptErrors.ReceivedDateInTheFuture);
        }

        return new GoodsReceipt(
            Guid.CreateVersion7(), number, purchaseOrderId, receivedByUserId,
            receivedOn, deliveryNoteReference, carrier);
    }

    public Result AddLine(
        Guid purchaseOrderLineId,
        decimal quantityAccepted,
        decimal quantityRejected,
        string? rejectionReason,
        string? notes)
    {
        if (IsReversed)
        {
            return Result.Failure(GoodsReceiptErrors.AlreadyReversed);
        }

        if (quantityAccepted < 0 || quantityRejected < 0)
        {
            return Result.Failure(GoodsReceiptErrors.QuantitiesMustNotBeNegative);
        }

        if (quantityAccepted == 0 && quantityRejected == 0)
        {
            return Result.Failure(GoodsReceiptErrors.NothingRecorded);
        }

        if (quantityRejected > 0 && string.IsNullOrWhiteSpace(rejectionReason))
        {
            return Result.Failure(GoodsReceiptErrors.RejectionReasonRequired);
        }

        if (_lines.Exists(l => l.PurchaseOrderLineId == purchaseOrderLineId))
        {
            return Result.Failure(GoodsReceiptErrors.DuplicateLine);
        }

        _lines.Add(GoodsReceiptLine.Create(
            Id, _lines.Count + 1, purchaseOrderLineId,
            quantityAccepted, quantityRejected, rejectionReason, notes));

        return Result.Success();
    }

    public Result Confirm()
    {
        if (_lines.Count == 0)
        {
            return Result.Failure(GoodsReceiptErrors.AtLeastOneLineRequired);
        }

        Raise(new GoodsReceiptRecorded(Id, Number, PurchaseOrderId));

        return Result.Success();
    }

    public Result Reverse(string reason)
    {
        if (IsReversed)
        {
            return Result.Failure(GoodsReceiptErrors.AlreadyReversed);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(GoodsReceiptErrors.ReversalReasonRequired);
        }

        IsReversed = true;
        ReversalReason = reason.Trim();
        Raise(new GoodsReceiptReversed(Id, Number, PurchaseOrderId, ReversalReason));

        return Result.Success();
    }

    /// <summary>
    /// The accepted quantities keyed by purchase order line, in the shape
    /// <see cref="PurchaseOrders.PurchaseOrder.ApplyReceipt"/> expects.
    /// </summary>
    public IReadOnlyDictionary<Guid, decimal> AcceptedQuantitiesByPoLine() =>
        _lines
            .Where(l => l.QuantityAccepted > 0)
            .ToDictionary(l => l.PurchaseOrderLineId, l => l.QuantityAccepted);
}
