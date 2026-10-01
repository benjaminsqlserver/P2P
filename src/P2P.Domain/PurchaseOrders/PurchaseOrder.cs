using P2P.Domain.Common;
using P2P.Domain.Requisitions;
using P2P.Domain.Vendors;

namespace P2P.Domain.PurchaseOrders;

public sealed class PurchaseOrder : AggregateRoot
{
    private readonly List<PurchaseOrderLine> _lines = [];

    private PurchaseOrder(
        Guid id,
        string number,
        Guid vendorId,
        Guid? requisitionId,
        Guid buyerId,
        string currency,
        DateOnly orderDate,
        DateOnly expectedDeliveryDate,
        int paymentTermsDays,
        Address shipToAddress)
        : base(id)
    {
        Number = number;
        VendorId = vendorId;
        RequisitionId = requisitionId;
        BuyerId = buyerId;
        Currency = currency;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        PaymentTermsDays = paymentTermsDays;
        ShipToAddress = shipToAddress;
        Status = PurchaseOrderStatus.Draft;
    }

    private PurchaseOrder() { }

    public string Number { get; private set; } = null!;

    public Guid VendorId { get; private set; }

    public Guid? RequisitionId { get; private set; }

    public Guid BuyerId { get; private set; }

    public string Currency { get; private set; } = null!;

    public DateOnly OrderDate { get; private set; }

    public DateOnly ExpectedDeliveryDate { get; private set; }

    public int PaymentTermsDays { get; private set; }

    public Address ShipToAddress { get; private set; } = null!;

    public PurchaseOrderStatus Status { get; private set; }

    public DateTimeOffset? IssuedAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    public Money NetTotal => _lines.Count == 0
        ? Money.Zero(Currency)
        : _lines.Aggregate(Money.Zero(Currency), (r, l) => r + l.NetTotal);

    public Money TaxTotal => _lines.Count == 0
        ? Money.Zero(Currency)
        : _lines.Aggregate(Money.Zero(Currency), (r, l) => r + l.TaxTotal);

    public Money GrossTotal => NetTotal + TaxTotal;

    public bool IsReceivable =>
        Status is PurchaseOrderStatus.Issued or PurchaseOrderStatus.PartiallyReceived;

    /// <summary>
    /// Creates a purchase order from an approved requisition, copying its
    /// lines. The vendor must be able to receive orders.
    /// </summary>
    public static Result<PurchaseOrder> CreateFromRequisition(
        string number,
        Requisition requisition,
        Vendor vendor,
        Guid buyerId,
        DateOnly orderDate,
        DateOnly expectedDeliveryDate,
        Address shipToAddress,
        decimal defaultTaxRatePercent)
    {
        if (requisition.Status != RequisitionStatus.Approved)
        {
            return Result.Failure<PurchaseOrder>(
                PurchaseOrderErrors.RequisitionNotApproved);
        }

        if (!vendor.CanReceivePurchaseOrders)
        {
            return Result.Failure<PurchaseOrder>(VendorErrors.NotActive);
        }

        if (!string.Equals(requisition.Currency, vendor.DefaultCurrency,
                StringComparison.Ordinal))
        {
            return Result.Failure<PurchaseOrder>(
                PurchaseOrderErrors.CurrencyMismatch);
        }

        var order = new PurchaseOrder(
            Guid.CreateVersion7(), number, vendor.Id, requisition.Id, buyerId,
            requisition.Currency, orderDate, expectedDeliveryDate,
            vendor.PaymentTermsDays, shipToAddress);

        foreach (var line in requisition.Lines.OrderBy(l => l.LineNumber))
        {
            order._lines.Add(PurchaseOrderLine.Create(
                order.Id,
                line.LineNumber,
                line.ItemCode,
                line.Description,
                line.Quantity,
                line.UnitOfMeasure,
                line.UnitPrice,
                defaultTaxRatePercent));
        }

        order.Raise(new PurchaseOrderCreated(
            order.Id, order.Number, vendor.Id, requisition.Id));

        return order;
    }

    public Result<PurchaseOrderLine> AddLine(
        string? itemCode,
        string description,
        decimal quantity,
        string unitOfMeasure,
        decimal unitPrice,
        decimal taxRatePercent)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            return Result.Failure<PurchaseOrderLine>(
                PurchaseOrderErrors.OnlyDraftCanBeEdited);
        }

        if (quantity <= 0)
        {
            return Result.Failure<PurchaseOrderLine>(
                PurchaseOrderErrors.LineQuantityMustBePositive);
        }

        var line = PurchaseOrderLine.Create(
            Id, _lines.Count + 1, itemCode, description, quantity,
            unitOfMeasure, new Money(unitPrice, Currency), taxRatePercent);

        _lines.Add(line);

        return line;
    }

    public Result Issue(DateTimeOffset nowUtc)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            return Result.Failure(PurchaseOrderErrors.AlreadyIssued);
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(PurchaseOrderErrors.AtLeastOneLineRequired);
        }

        Status = PurchaseOrderStatus.Issued;
        IssuedAtUtc = nowUtc;
        Raise(new PurchaseOrderIssued(
            Id, Number, VendorId, GrossTotal.Amount, Currency));

        return Result.Success();
    }

    /// <summary>
    /// Applies a set of received quantities, keyed by purchase order line id.
    /// Called by the goods receipt workflow; the receipt document itself is a
    /// separate aggregate.
    /// </summary>
    public Result ApplyReceipt(
        IReadOnlyDictionary<Guid, decimal> quantitiesByLineId,
        decimal overReceiptTolerancePercent)
    {
        if (!IsReceivable)
        {
            return Result.Failure(PurchaseOrderErrors.NotReceivable);
        }

        foreach (var (lineId, quantity) in quantitiesByLineId)
        {
            var line = _lines.SingleOrDefault(l => l.Id == lineId);

            if (line is null)
            {
                return Result.Failure(Error.NotFound("PurchaseOrderLine", lineId));
            }

            var result = line.RecordReceipt(quantity, overReceiptTolerancePercent);

            if (result.IsFailure)
            {
                return result;
            }
        }

        Status = _lines.TrueForAll(l => l.IsFullyReceived)
            ? PurchaseOrderStatus.Received
            : PurchaseOrderStatus.PartiallyReceived;

        Raise(new PurchaseOrderReceiptApplied(Id, Number, Status));

        return Result.Success();
    }

    public Result ApplyInvoiceMatch(IReadOnlyDictionary<Guid, decimal> quantitiesByLineId)
    {
        foreach (var (lineId, quantity) in quantitiesByLineId)
        {
            var line = _lines.SingleOrDefault(l => l.Id == lineId);

            if (line is null)
            {
                return Result.Failure(Error.NotFound("PurchaseOrderLine", lineId));
            }

            var result = line.RecordInvoiced(quantity);

            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }

    public Result Close()
    {
        if (Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Closed)
        {
            return Result.Failure(PurchaseOrderErrors.CannotClose);
        }

        Status = PurchaseOrderStatus.Closed;
        Raise(new PurchaseOrderClosed(Id, Number));

        return Result.Success();
    }

    public Result Cancel(string reason, DateTimeOffset nowUtc)
    {
        if (_lines.Exists(l => l.QuantityReceived > 0))
        {
            return Result.Failure(PurchaseOrderErrors.CannotCancelAfterReceipt);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(PurchaseOrderErrors.CancellationReasonRequired);
        }

        Status = PurchaseOrderStatus.Cancelled;
        CancellationReason = reason.Trim();
        Raise(new PurchaseOrderCancelled(Id, Number, CancellationReason));

        return Result.Success();
    }
}
