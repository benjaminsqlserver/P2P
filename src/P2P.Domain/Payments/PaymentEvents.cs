using P2P.Domain.Common;

namespace P2P.Domain.Payments;

public sealed record PaymentAuthorised(
    Guid PaymentId, string Number, Guid VendorId, decimal Total, string Currency)
    : DomainEvent;

public sealed record PaymentExecuted(
    Guid PaymentId, string Number, Guid VendorId, decimal Total, string Reference)
    : DomainEvent;

public sealed record PaymentFailed(
    Guid PaymentId, string Number, Guid VendorId, string Reason) : DomainEvent;
