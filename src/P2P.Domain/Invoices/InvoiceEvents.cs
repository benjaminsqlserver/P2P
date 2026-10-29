using P2P.Domain.Common;

namespace P2P.Domain.Invoices;

public sealed record InvoiceReceived(
    Guid InvoiceId, string InternalNumber, Guid VendorId, Guid PurchaseOrderId)
    : DomainEvent;

public sealed record InvoiceMatched(
    Guid InvoiceId, string InternalNumber, Guid VendorId, decimal GrossTotal)
    : DomainEvent;

public sealed record InvoiceMatchFailed(
    Guid InvoiceId, string InternalNumber, Guid VendorId, int ExceptionCount)
    : DomainEvent;

public sealed record InvoiceApprovedForPayment(
    Guid InvoiceId, string InternalNumber, Guid VendorId,
    decimal GrossTotal, string Currency, DateOnly DueDate) : DomainEvent;

public sealed record InvoiceDisputed(
    Guid InvoiceId, string InternalNumber, Guid VendorId, string Reason)
    : DomainEvent;

public sealed record InvoiceSettled(
    Guid InvoiceId, string InternalNumber, Guid VendorId, DateTimeOffset SettledAtUtc)
    : DomainEvent;
