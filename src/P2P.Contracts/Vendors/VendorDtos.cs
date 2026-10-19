namespace P2P.Contracts.Vendors;

public enum VendorStatusDto
{
    Draft = 0,
    Active = 1,
    OnHold = 2,
    Blocked = 3
}

public sealed record AddressDto
{
    public required string Line1 { get; init; }
    public string? Line2 { get; init; }
    public required string City { get; init; }
    public string? StateOrProvince { get; init; }
    public required string PostalCode { get; init; }
    public required string CountryCode { get; init; }
}

/// <summary>Row shape for the vendor register grid.</summary>
public sealed record VendorListItemDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string LegalName { get; init; }
    public string? TradingName { get; init; }
    public required VendorStatusDto Status { get; init; }
    public required string DefaultCurrency { get; init; }
    public required int PaymentTermsDays { get; init; }
    public required string City { get; init; }
    public required string CountryCode { get; init; }
    public required int OpenPurchaseOrderCount { get; init; }
    public required decimal OutstandingBalance { get; init; }
}

/// <summary>Full detail for the vendor edit form.</summary>
public sealed record VendorDetailDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string LegalName { get; init; }
    public string? TradingName { get; init; }
    public string? TaxIdentifier { get; init; }
    public required AddressDto RemitToAddress { get; init; }
    public required string DefaultCurrency { get; init; }
    public required int PaymentTermsDays { get; init; }
    public required VendorStatusDto Status { get; init; }
    public string? HoldReason { get; init; }
    public required IReadOnlyList<BankAccountDto> BankAccounts { get; init; }
}

public sealed record BankAccountDto
{
    public required Guid Id { get; init; }
    public required string BankName { get; init; }
    public required string AccountName { get; init; }
    public required string AccountNumber { get; init; }
    public string? SortCodeOrSwift { get; init; }
    public required string Currency { get; init; }
    public required bool IsPrimary { get; init; }
}

public sealed record CreateVendorRequest
{
    public required string Code { get; init; }
    public required string LegalName { get; init; }
    public string? TradingName { get; init; }
    public string? TaxIdentifier { get; init; }
    public required AddressDto RemitToAddress { get; init; }
    public required string DefaultCurrency { get; init; }
    public required int PaymentTermsDays { get; init; }
}

public sealed record UpdateVendorRequest
{
    public required Guid Id { get; init; }
    public required string LegalName { get; init; }
    public string? TradingName { get; init; }
    public string? TaxIdentifier { get; init; }
    public required AddressDto RemitToAddress { get; init; }
    public required int PaymentTermsDays { get; init; }
}

public sealed record AddBankAccountRequest
{
    public required Guid VendorId { get; init; }
    public required string BankName { get; init; }
    public required string AccountName { get; init; }
    public required string AccountNumber { get; init; }
    public string? SortCodeOrSwift { get; init; }
    public required string Currency { get; init; }
    public required bool MakePrimary { get; init; }
}

public sealed record VendorGridQuery : Common.GridQuery
{
    public VendorStatusDto? Status { get; init; }
    public string? CountryCode { get; init; }
}

/// <summary>Lightweight option for dropdowns.</summary>
public sealed record VendorLookupDto(
    Guid Id, string Code, string LegalName, string DefaultCurrency);
