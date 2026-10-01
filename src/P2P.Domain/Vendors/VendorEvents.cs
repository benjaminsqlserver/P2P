using P2P.Domain.Common;

namespace P2P.Domain.Vendors;

public sealed record VendorRegistered(Guid VendorId, string Code, string LegalName)
    : DomainEvent;

public sealed record VendorActivated(Guid VendorId, string Code) : DomainEvent;

public sealed record VendorPlacedOnHold(Guid VendorId, string Code, string Reason)
    : DomainEvent;

public sealed record VendorBlocked(Guid VendorId, string Code, string Reason)
    : DomainEvent;

public sealed record VendorBankAccountAdded(
    Guid VendorId, Guid BankAccountId, string AccountNumber) : DomainEvent;
