using P2P.Domain.Common;

namespace P2P.Domain.GoodsReceipts;

public sealed class GoodsReceiptLine : Entity
{
    private GoodsReceiptLine(
        Guid id,
        Guid goodsReceiptId,
        int lineNumber,
        Guid purchaseOrderLineId,
        decimal quantityAccepted,
        decimal quantityRejected,
        string? rejectionReason,
        string? notes)
        : base(id)
    {
        GoodsReceiptId = goodsReceiptId;
        LineNumber = lineNumber;
        PurchaseOrderLineId = purchaseOrderLineId;
        QuantityAccepted = quantityAccepted;
        QuantityRejected = quantityRejected;
        RejectionReason = rejectionReason;
        Notes = notes;
    }

    private GoodsReceiptLine() { }

    public Guid GoodsReceiptId { get; private set; }

    public int LineNumber { get; private set; }

    public Guid PurchaseOrderLineId { get; private set; }

    public decimal QuantityAccepted { get; private set; }

    public decimal QuantityRejected { get; private set; }

    public string? RejectionReason { get; private set; }

    public string? Notes { get; private set; }

    public decimal QuantityDelivered => QuantityAccepted + QuantityRejected;

    internal static GoodsReceiptLine Create(
        Guid goodsReceiptId,
        int lineNumber,
        Guid purchaseOrderLineId,
        decimal quantityAccepted,
        decimal quantityRejected,
        string? rejectionReason,
        string? notes) =>
        new(Guid.CreateVersion7(), goodsReceiptId, lineNumber, purchaseOrderLineId,
            quantityAccepted, quantityRejected, rejectionReason, notes);
}
