namespace P2P.Contracts.GoodsReceipts;

public sealed record GoodsReceiptLineRequest
{
    public required Guid PurchaseOrderLineId { get; init; }
    public required decimal QuantityAccepted { get; init; }
    public required decimal QuantityRejected { get; init; }
    public string? RejectionReason { get; init; }
    public string? Notes { get; init; }
}

public sealed record GoodsReceiptLineDto
{
    public required int LineNumber { get; init; }
    public required Guid PurchaseOrderLineId { get; init; }
    public required string Description { get; init; }
    public required decimal QuantityAccepted { get; init; }
    public required decimal QuantityRejected { get; init; }
    public string? RejectionReason { get; init; }
}

public sealed record GoodsReceiptDetailDto
{
    public required Guid Id { get; init; }
    public required string Number { get; init; }
    public required Guid PurchaseOrderId { get; init; }
    public required string PurchaseOrderNumber { get; init; }
    public required string VendorName { get; init; }
    public required DateOnly ReceivedOn { get; init; }
    public string? DeliveryNoteReference { get; init; }
    public string? Carrier { get; init; }
    public required bool IsReversed { get; init; }
    public string? ReversalReason { get; init; }
    public required IReadOnlyList<GoodsReceiptLineDto> Lines { get; init; }
}

public sealed record CreateGoodsReceiptRequest
{
    public required Guid PurchaseOrderId { get; init; }
    public required DateOnly ReceivedOn { get; init; }
    public string? DeliveryNoteReference { get; init; }
    public string? Carrier { get; init; }
    public required IReadOnlyList<GoodsReceiptLineRequest> Lines { get; init; }
}
