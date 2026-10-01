using P2P.Domain.Common;

namespace P2P.Domain.Payments;

public enum PaymentMethod
{
    BankTransfer = 0,
    Cheque = 1,
    DirectDebit = 2,
    Card = 3,
    Cash = 4
}

public enum PaymentStatus
{
    Draft = 0,
    Authorised = 1,
    Executed = 2,
    Failed = 3,
    Cancelled = 4
}

public sealed class Payment : AggregateRoot
{
    private readonly List<PaymentAllocation> _allocations = [];

    private Payment(
        Guid id,
        string number,
        Guid vendorId,
        Guid vendorBankAccountId,
        DateOnly paymentDate,
        PaymentMethod method,
        string currency)
        : base(id)
    {
        Number = number;
        VendorId = vendorId;
        VendorBankAccountId = vendorBankAccountId;
        PaymentDate = paymentDate;
        Method = method;
        Currency = currency;
        Status = PaymentStatus.Draft;
    }

    private Payment() { }

    public string Number { get; private set; } = null!;

    public Guid VendorId { get; private set; }

    public Guid VendorBankAccountId { get; private set; }

    public DateOnly PaymentDate { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string Currency { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    public string? Reference { get; private set; }

    public Guid? AuthorisedByUserId { get; private set; }

    public DateTimeOffset? AuthorisedAtUtc { get; private set; }

    public IReadOnlyCollection<PaymentAllocation> Allocations =>
        _allocations.AsReadOnly();

    public Money Total => _allocations.Count == 0
        ? Money.Zero(Currency)
        : _allocations.Aggregate(
            Money.Zero(Currency), (running, a) => running + a.Amount);

    public static Result<Payment> Create(
        string number,
        Guid vendorId,
        Guid vendorBankAccountId,
        DateOnly paymentDate,
        PaymentMethod method,
        string currency) =>
        new Payment(Guid.CreateVersion7(), number, vendorId, vendorBankAccountId,
            paymentDate, method, currency.ToUpperInvariant());

    public Result Allocate(Guid invoiceId, decimal amount)
    {
        if (Status != PaymentStatus.Draft)
        {
            return Result.Failure(PaymentErrors.OnlyDraftCanBeEdited);
        }

        if (amount <= 0)
        {
            return Result.Failure(PaymentErrors.AllocationMustBePositive);
        }

        if (_allocations.Exists(a => a.InvoiceId == invoiceId))
        {
            return Result.Failure(PaymentErrors.DuplicateAllocation);
        }

        _allocations.Add(PaymentAllocation.Create(
            Id, invoiceId, new Money(amount, Currency)));

        return Result.Success();
    }

    public Result Authorise(Guid userId, DateTimeOffset nowUtc)
    {
        if (Status != PaymentStatus.Draft)
        {
            return Result.Failure(PaymentErrors.OnlyDraftCanBeAuthorised);
        }

        if (_allocations.Count == 0)
        {
            return Result.Failure(PaymentErrors.NothingAllocated);
        }

        Status = PaymentStatus.Authorised;
        AuthorisedByUserId = userId;
        AuthorisedAtUtc = nowUtc;
        Raise(new PaymentAuthorised(Id, Number, VendorId, Total.Amount, Currency));

        return Result.Success();
    }

    public Result MarkExecuted(string reference)
    {
        if (Status != PaymentStatus.Authorised)
        {
            return Result.Failure(PaymentErrors.OnlyAuthorisedCanBeExecuted);
        }

        Status = PaymentStatus.Executed;
        Reference = reference;
        Raise(new PaymentExecuted(Id, Number, VendorId, Total.Amount, reference));

        return Result.Success();
    }

    public Result MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        Reference = reason;
        Raise(new PaymentFailed(Id, Number, VendorId, reason));

        return Result.Success();
    }
}

public sealed class PaymentAllocation : Entity
{
    private PaymentAllocation(Guid id, Guid paymentId, Guid invoiceId, Money amount)
        : base(id)
    {
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        Amount = amount;
    }

    private PaymentAllocation() { }

    public Guid PaymentId { get; private set; }

    public Guid InvoiceId { get; private set; }

    public Money Amount { get; private set; }

    internal static PaymentAllocation Create(
        Guid paymentId, Guid invoiceId, Money amount) =>
        new(Guid.CreateVersion7(), paymentId, invoiceId, amount);
}
