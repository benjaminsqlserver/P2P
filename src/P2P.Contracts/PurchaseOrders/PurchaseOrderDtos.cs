using P2P.Contracts.Vendors;

namespace P2P.Contracts.PurchaseOrders;

public enum PurchaseOrderStatusDto
{
    Draft = 0,
    Issued = 1,
    PartiallyReceived = 2,
    Received = 3,
    Closed = 4,
    Cancelled = 5
}

/// <summary>Option for the receiving clerk's open-order picker.</summary>
public sealed record PurchaseOrderLookupDto(
    Guid Id, string Number, string VendorName, DateOnly ExpectedDeliveryDate);

public sealed record PurchaseOrderLineDto
{
    public required Guid Id { get; init; }
    public required int LineNumber { get; init; }
    public string? ItemCode { get; init; }
    public required string Description { get; init; }
    public required decimal QuantityOrdered { get; init; }
    public required string UnitOfMeasure { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal TaxRatePercent { get; init; }
    public required decimal QuantityReceived { get; init; }
    public required decimal QuantityInvoiced { get; init; }
    public required decimal NetTotal { get; init; }
}

public sealed record PurchaseOrderDetailDto
{
    public required Guid Id { get; init; }
    public required string Number { get; init; }
    public required Guid VendorId { get; init; }
    public required string VendorName { get; init; }
    public Guid? RequisitionId { get; init; }
    public required PurchaseOrderStatusDto Status { get; init; }
    public required string Currency { get; init; }
    public required DateOnly OrderDate { get; init; }
    public required DateOnly ExpectedDeliveryDate { get; init; }
    public required int PaymentTermsDays { get; init; }
    public required AddressDto ShipToAddress { get; init; }
    public required decimal NetTotal { get; init; }
    public required decimal TaxTotal { get; init; }
    public required decimal GrossTotal { get; init; }
    public required IReadOnlyList<PurchaseOrderLineDto> Lines { get; init; }
}

public sealed record CreatePurchaseOrderFromRequisitionRequest
{
    public required Guid RequisitionId { get; init; }
    public required Guid VendorId { get; init; }
    public required DateOnly ExpectedDeliveryDate { get; init; }
    public required AddressDto ShipToAddress { get; init; }
    public required decimal TaxRatePercent { get; init; }
}
