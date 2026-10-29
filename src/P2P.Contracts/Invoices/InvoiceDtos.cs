namespace P2P.Contracts.Invoices;

public enum InvoiceStatusDto
{
    Received = 0,
    Matched = 1,
    Exception = 2,
    Approved = 3,
    PartiallyPaid = 4,
    Paid = 5,
    Disputed = 6,
    Cancelled = 7
}

public enum MatchExceptionTypeDto
{
    QuantityOverBilled = 0,
    PriceAboveTolerance = 1,
    NoReceiptRecorded = 2,
    LineNotOnPurchaseOrder = 3,
    TotalMismatch = 4,
    DuplicateInvoiceNumber = 5,
    CurrencyMismatch = 6
}

public sealed record InvoiceExceptionQuery : Common.GridQuery
{
    public MatchExceptionTypeDto? ExceptionType { get; init; }
}

/// <summary>Row shape for the match exception workbench.</summary>
public sealed record InvoiceExceptionRowDto
{
    public required Guid InvoiceId { get; init; }
    public required string InternalNumber { get; init; }
    public required string VendorInvoiceNumber { get; init; }
    public required string VendorName { get; init; }
    public required string PurchaseOrderNumber { get; init; }
    public required IReadOnlyList<MatchExceptionTypeDto> ExceptionTypes { get; init; }
    public required decimal GrossTotal { get; init; }
    public required string Currency { get; init; }
    public required int AgeInDays { get; init; }
}

public sealed record MatchExceptionDto
{
    public required Guid Id { get; init; }
    public Guid? InvoiceLineId { get; init; }
    public required MatchExceptionTypeDto Type { get; init; }
    public required string Description { get; init; }
    public decimal? ExpectedValue { get; init; }
    public decimal? ActualValue { get; init; }
    public required bool IsResolved { get; init; }
    public string? Resolution { get; init; }
    public DateTimeOffset? ResolvedAtUtc { get; init; }
}

/// <summary>One row of the three-way comparison: PO, receipt and invoice side by side.</summary>
public sealed record MatchComparisonLineDto
{
    public required Guid InvoiceLineId { get; init; }
    public required int LineNumber { get; init; }
    public required string Description { get; init; }
    public required decimal QuantityOrdered { get; init; }
    public required decimal PoUnitPrice { get; init; }
    public required decimal QuantityReceived { get; init; }
    public required decimal QuantityInvoiced { get; init; }
    public required decimal InvoiceUnitPrice { get; init; }
}

public sealed record InvoiceLineDto
{
    public required Guid Id { get; init; }
    public required int LineNumber { get; init; }
    public Guid? PurchaseOrderLineId { get; init; }
    public required string Description { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal TaxRatePercent { get; init; }
    public required decimal NetTotal { get; init; }
}

public sealed record InvoiceDetailDto
{
    public required Guid Id { get; init; }
    public required string InternalNumber { get; init; }
    public required string VendorInvoiceNumber { get; init; }
    public required Guid VendorId { get; init; }
    public required string VendorName { get; init; }
    public required Guid PurchaseOrderId { get; init; }
    public required string PurchaseOrderNumber { get; init; }
    public required DateOnly InvoiceDate { get; init; }
    public required DateOnly DueDate { get; init; }
    public required string Currency { get; init; }
    public required InvoiceStatusDto Status { get; init; }
    public required decimal NetTotal { get; init; }
    public required decimal TaxTotal { get; init; }
    public required decimal GrossTotal { get; init; }
    public required decimal AmountPaid { get; init; }
    public required IReadOnlyList<InvoiceLineDto> Lines { get; init; }
    public required IReadOnlyList<MatchExceptionDto> MatchExceptions { get; init; }
    public required IReadOnlyList<MatchComparisonLineDto> Comparison { get; init; }
}
