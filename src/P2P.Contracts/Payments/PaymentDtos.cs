namespace P2P.Contracts.Payments;

public enum PaymentMethodDto
{
    BankTransfer = 0,
    Cheque = 1,
    DirectDebit = 2,
    Card = 3,
    Cash = 4
}

public enum PaymentStatusDto
{
    Draft = 0,
    Authorised = 1,
    Executed = 2,
    Failed = 3,
    Cancelled = 4
}

/// <summary>Row shape for the payment proposal grid.</summary>
public sealed record PayableInvoiceDto
{
    public required Guid InvoiceId { get; init; }
    public required Guid VendorId { get; init; }
    public required string VendorName { get; init; }
    public required string InternalNumber { get; init; }
    public required string VendorInvoiceNumber { get; init; }
    public required DateOnly DueDate { get; init; }
    public required int DaysUntilDue { get; init; }
    public required decimal AmountOutstanding { get; init; }
    public required string Currency { get; init; }
}

public sealed record CreatePaymentRunRequest
{
    public required DateOnly PaymentDate { get; init; }
    public required PaymentMethodDto Method { get; init; }
    public required IReadOnlyList<Guid> InvoiceIds { get; init; }
}
