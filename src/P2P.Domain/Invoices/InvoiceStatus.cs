namespace P2P.Domain.Invoices;

public enum InvoiceStatus
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

public enum MatchExceptionType
{
    QuantityOverBilled = 0,
    PriceAboveTolerance = 1,
    NoReceiptRecorded = 2,
    LineNotOnPurchaseOrder = 3,
    TotalMismatch = 4,
    DuplicateInvoiceNumber = 5,
    CurrencyMismatch = 6
}
