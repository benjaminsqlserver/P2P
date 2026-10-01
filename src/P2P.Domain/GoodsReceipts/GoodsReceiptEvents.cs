using P2P.Domain.Common;

namespace P2P.Domain.GoodsReceipts;

public sealed record GoodsReceiptRecorded(
    Guid GoodsReceiptId, string Number, Guid PurchaseOrderId) : DomainEvent;

public sealed record GoodsReceiptReversed(
    Guid GoodsReceiptId, string Number, Guid PurchaseOrderId, string Reason)
    : DomainEvent;
