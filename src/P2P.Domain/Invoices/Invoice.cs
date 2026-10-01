using P2P.Domain.Common;

namespace P2P.Domain.Invoices;

public sealed class Invoice : AggregateRoot
{
    private readonly List<InvoiceLine> _lines = [];
    private readonly List<MatchException> _matchExceptions = [];

    private Invoice(
        Guid id,
        string internalNumber,
        string vendorInvoiceNumber,
        Guid vendorId,
        Guid purchaseOrderId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        string currency)
        : base(id)
    {
        InternalNumber = internalNumber;
        VendorInvoiceNumber = vendorInvoiceNumber;
        VendorId = vendorId;
        PurchaseOrderId = purchaseOrderId;
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        Currency = currency;
        Status = InvoiceStatus.Received;
        AmountPaid = Money.Zero(currency);
    }

    private Invoice() { }

    public string InternalNumber { get; private set; } = null!;

    public string VendorInvoiceNumber { get; private set; } = null!;

    public Guid VendorId { get; private set; }

    public Guid PurchaseOrderId { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public DateOnly DueDate { get; private set; }

    public string Currency { get; private set; } = null!;

    public InvoiceStatus Status { get; private set; }

    public Money AmountPaid { get; private set; }

    public DateTimeOffset? MatchedAtUtc { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public Guid? ApprovedByUserId { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public IReadOnlyCollection<MatchException> MatchExceptions =>
        _matchExceptions.AsReadOnly();

    public Money NetTotal => _lines.Count == 0
        ? Money.Zero(Currency)
        : _lines.Aggregate(Money.Zero(Currency), (r, l) => r + l.NetTotal);

    public Money TaxTotal => _lines.Count == 0
        ? Money.Zero(Currency)
        : _lines.Aggregate(Money.Zero(Currency), (r, l) => r + l.TaxTotal);

    public Money GrossTotal => NetTotal + TaxTotal;

    public Money AmountOutstanding => GrossTotal - AmountPaid;

    public bool HasUnresolvedExceptions =>
        _matchExceptions.Exists(e => !e.IsResolved);

    public bool IsPayable => Status == InvoiceStatus.Approved
                             || Status == InvoiceStatus.PartiallyPaid;

    public static Result<Invoice> Create(
        string internalNumber,
        string vendorInvoiceNumber,
        Guid vendorId,
        Guid purchaseOrderId,
        DateOnly invoiceDate,
        DateOnly dueDate,
        string currency)
    {
        if (string.IsNullOrWhiteSpace(vendorInvoiceNumber))
        {
            return Result.Failure<Invoice>(InvoiceErrors.VendorInvoiceNumberRequired);
        }

        if (dueDate < invoiceDate)
        {
            return Result.Failure<Invoice>(InvoiceErrors.DueDateBeforeInvoiceDate);
        }

        var invoice = new Invoice(
            Guid.CreateVersion7(), internalNumber, vendorInvoiceNumber.Trim(),
            vendorId, purchaseOrderId, invoiceDate, dueDate,
            currency.ToUpperInvariant());

        invoice.Raise(new InvoiceReceived(
            invoice.Id, invoice.InternalNumber, vendorId, purchaseOrderId));

        return invoice;
    }

    public Result AddLine(
        Guid? purchaseOrderLineId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRatePercent)
    {
        if (Status is not (InvoiceStatus.Received or InvoiceStatus.Exception))
        {
            return Result.Failure(InvoiceErrors.NotEditable);
        }

        if (quantity <= 0)
        {
            return Result.Failure(InvoiceErrors.LineQuantityMustBePositive);
        }

        _lines.Add(InvoiceLine.Create(
            Id, _lines.Count + 1, purchaseOrderLineId, description,
            quantity, new Money(unitPrice, Currency), taxRatePercent));

        return Result.Success();
    }

    /// <summary>
    /// Records the outcome of a three-way match run. Called only by
    /// <see cref="ThreeWayMatchService"/>.
    /// </summary>
    internal void ApplyMatchResult(
        IReadOnlyList<MatchException> exceptions,
        DateTimeOffset nowUtc)
    {
        _matchExceptions.Clear();
        _matchExceptions.AddRange(exceptions);
        MatchedAtUtc = nowUtc;

        if (exceptions.Count == 0)
        {
            Status = InvoiceStatus.Matched;
            Raise(new InvoiceMatched(Id, InternalNumber, VendorId, GrossTotal.Amount));
        }
        else
        {
            Status = InvoiceStatus.Exception;
            Raise(new InvoiceMatchFailed(
                Id, InternalNumber, VendorId, exceptions.Count));
        }
    }

    public Result ResolveException(
        Guid exceptionId, Guid userId, string resolution, DateTimeOffset nowUtc)
    {
        var exception = _matchExceptions.SingleOrDefault(e => e.Id == exceptionId);

        if (exception is null)
        {
            return Result.Failure(Error.NotFound("MatchException", exceptionId));
        }

        if (string.IsNullOrWhiteSpace(resolution))
        {
            return Result.Failure(InvoiceErrors.ResolutionRequired);
        }

        exception.Resolve(userId, resolution.Trim(), nowUtc);

        if (!HasUnresolvedExceptions)
        {
            Status = InvoiceStatus.Matched;
            Raise(new InvoiceMatched(Id, InternalNumber, VendorId, GrossTotal.Amount));
        }

        return Result.Success();
    }

    public Result Approve(Guid approverId, DateTimeOffset nowUtc)
    {
        if (Status != InvoiceStatus.Matched)
        {
            return Result.Failure(InvoiceErrors.OnlyMatchedCanBeApproved);
        }

        if (HasUnresolvedExceptions)
        {
            return Result.Failure(InvoiceErrors.UnresolvedExceptions);
        }

        Status = InvoiceStatus.Approved;
        ApprovedByUserId = approverId;
        ApprovedAtUtc = nowUtc;
        Raise(new InvoiceApprovedForPayment(
            Id, InternalNumber, VendorId, GrossTotal.Amount, Currency, DueDate));

        return Result.Success();
    }

    public Result Dispute(string reason)
    {
        if (Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
        {
            return Result.Failure(InvoiceErrors.CannotDispute);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(InvoiceErrors.DisputeReasonRequired);
        }

        Status = InvoiceStatus.Disputed;
        Raise(new InvoiceDisputed(Id, InternalNumber, VendorId, reason.Trim()));

        return Result.Success();
    }

    /// <summary>
    /// Applies a payment allocation. Called from the payment aggregate's
    /// workflow; the payment document itself is a separate aggregate.
    /// </summary>
    public Result ApplyPayment(Money amount, DateTimeOffset nowUtc)
    {
        if (!IsPayable)
        {
            return Result.Failure(InvoiceErrors.NotPayable);
        }

        if (amount.IsNegative || amount.IsZero)
        {
            return Result.Failure(InvoiceErrors.PaymentAmountMustBePositive);
        }

        if (amount > AmountOutstanding)
        {
            return Result.Failure(InvoiceErrors.OverPayment);
        }

        AmountPaid += amount;

        Status = AmountOutstanding.IsZero
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;

        if (Status == InvoiceStatus.Paid)
        {
            Raise(new InvoiceSettled(Id, InternalNumber, VendorId, nowUtc));
        }

        return Result.Success();
    }

    public IReadOnlyDictionary<Guid, decimal> InvoicedQuantitiesByPoLine() =>
        _lines
            .Where(l => l.PurchaseOrderLineId.HasValue)
            .GroupBy(l => l.PurchaseOrderLineId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
}
