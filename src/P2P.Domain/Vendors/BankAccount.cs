using P2P.Domain.Common;

namespace P2P.Domain.Vendors;

public sealed class BankAccount : Entity
{
    private BankAccount(
        Guid id,
        Guid vendorId,
        string bankName,
        string accountName,
        string accountNumber,
        string? sortCodeOrSwift,
        string currency)
        : base(id)
    {
        VendorId = vendorId;
        BankName = bankName;
        AccountName = accountName;
        AccountNumber = accountNumber;
        SortCodeOrSwift = sortCodeOrSwift;
        Currency = currency;
        IsPrimary = false;
    }

    private BankAccount() { }

    public Guid VendorId { get; private set; }

    public string BankName { get; private set; } = null!;

    public string AccountName { get; private set; } = null!;

    public string AccountNumber { get; private set; } = null!;

    public string? SortCodeOrSwift { get; private set; }

    public string Currency { get; private set; } = null!;

    public bool IsPrimary { get; private set; }

    internal static BankAccount Create(
        Guid vendorId,
        string bankName,
        string accountName,
        string accountNumber,
        string? sortCodeOrSwift,
        string currency) =>
        new(Guid.CreateVersion7(), vendorId, bankName, accountName,
            accountNumber, sortCodeOrSwift, currency);

    internal void MarkPrimary() => IsPrimary = true;

    internal void ClearPrimary() => IsPrimary = false;
}
