using P2P.Domain.Common;

namespace P2P.Domain.Vendors;

public sealed class Vendor : AggregateRoot
{
    private readonly List<BankAccount> _bankAccounts = [];

    private Vendor(
        Guid id,
        string code,
        string legalName,
        string? tradingName,
        string? taxIdentifier,
        Address remitToAddress,
        string defaultCurrency,
        int paymentTermsDays)
        : base(id)
    {
        Code = code;
        LegalName = legalName;
        TradingName = tradingName;
        TaxIdentifier = taxIdentifier;
        RemitToAddress = remitToAddress;
        DefaultCurrency = defaultCurrency;
        PaymentTermsDays = paymentTermsDays;
        Status = VendorStatus.Draft;
    }

    private Vendor() { }

    public string Code { get; private set; } = null!;

    public string LegalName { get; private set; } = null!;

    public string? TradingName { get; private set; }

    public string? TaxIdentifier { get; private set; }

    public Address RemitToAddress { get; private set; } = null!;

    public string DefaultCurrency { get; private set; } = null!;

    public int PaymentTermsDays { get; private set; }

    public VendorStatus Status { get; private set; }

    public string? HoldReason { get; private set; }

    public IReadOnlyCollection<BankAccount> BankAccounts =>
        _bankAccounts.AsReadOnly();

    public bool CanReceivePurchaseOrders => Status == VendorStatus.Active;

    public static Result<Vendor> Create(
        string code,
        string legalName,
        string? tradingName,
        string? taxIdentifier,
        Address remitToAddress,
        string defaultCurrency,
        int paymentTermsDays)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Vendor>(VendorErrors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure<Vendor>(VendorErrors.LegalNameRequired);
        }

        if (paymentTermsDays is < 0 or > 365)
        {
            return Result.Failure<Vendor>(VendorErrors.PaymentTermsOutOfRange);
        }

        var vendor = new Vendor(
            Guid.CreateVersion7(),
            code.Trim().ToUpperInvariant(),
            legalName.Trim(),
            string.IsNullOrWhiteSpace(tradingName) ? null : tradingName.Trim(),
            string.IsNullOrWhiteSpace(taxIdentifier) ? null : taxIdentifier.Trim(),
            remitToAddress,
            defaultCurrency.ToUpperInvariant(),
            paymentTermsDays);

        vendor.Raise(new VendorRegistered(vendor.Id, vendor.Code, vendor.LegalName));

        return vendor;
    }

    public Result UpdateDetails(
        string legalName,
        string? tradingName,
        string? taxIdentifier,
        Address remitToAddress,
        int paymentTermsDays)
    {
        if (Status == VendorStatus.Blocked)
        {
            return Result.Failure(VendorErrors.CannotEditBlockedVendor);
        }

        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure(VendorErrors.LegalNameRequired);
        }

        if (paymentTermsDays is < 0 or > 365)
        {
            return Result.Failure(VendorErrors.PaymentTermsOutOfRange);
        }

        LegalName = legalName.Trim();
        TradingName = string.IsNullOrWhiteSpace(tradingName) ? null : tradingName.Trim();
        TaxIdentifier = string.IsNullOrWhiteSpace(taxIdentifier) ? null : taxIdentifier.Trim();
        RemitToAddress = remitToAddress;
        PaymentTermsDays = paymentTermsDays;

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == VendorStatus.Active)
        {
            return Result.Success();
        }

        if (Status == VendorStatus.Blocked)
        {
            return Result.Failure(VendorErrors.CannotActivateBlockedVendor);
        }

        if (_bankAccounts.Count == 0)
        {
            return Result.Failure(VendorErrors.BankAccountRequiredForActivation);
        }

        Status = VendorStatus.Active;
        HoldReason = null;
        Raise(new VendorActivated(Id, Code));

        return Result.Success();
    }

    public Result PlaceOnHold(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(VendorErrors.HoldReasonRequired);
        }

        Status = VendorStatus.OnHold;
        HoldReason = reason.Trim();
        Raise(new VendorPlacedOnHold(Id, Code, HoldReason));

        return Result.Success();
    }

    public Result Block(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(VendorErrors.HoldReasonRequired);
        }

        Status = VendorStatus.Blocked;
        HoldReason = reason.Trim();
        Raise(new VendorBlocked(Id, Code, HoldReason));

        return Result.Success();
    }

    public Result<BankAccount> AddBankAccount(
        string bankName,
        string accountName,
        string accountNumber,
        string? sortCodeOrSwift,
        string currency,
        bool makePrimary)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return Result.Failure<BankAccount>(VendorErrors.AccountNumberRequired);
        }

        var normalised = accountNumber.Replace(" ", string.Empty, StringComparison.Ordinal);

        if (_bankAccounts.Any(a =>
                a.AccountNumber.Equals(normalised, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<BankAccount>(VendorErrors.DuplicateBankAccount);
        }

        var account = BankAccount.Create(
            Id, bankName, accountName, normalised, sortCodeOrSwift, currency);

        if (makePrimary || _bankAccounts.Count == 0)
        {
            foreach (var existing in _bankAccounts)
            {
                existing.ClearPrimary();
            }

            account.MarkPrimary();
        }

        _bankAccounts.Add(account);
        Raise(new VendorBankAccountAdded(Id, account.Id, normalised));

        return account;
    }
}
