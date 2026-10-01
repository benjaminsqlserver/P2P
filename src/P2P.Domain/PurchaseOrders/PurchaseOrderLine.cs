using P2P.Domain.Common;

namespace P2P.Domain.PurchaseOrders;

public sealed class PurchaseOrderLine : Entity
{
    private PurchaseOrderLine(
        Guid id,
        Guid purchaseOrderId,
        int lineNumber,
        string? itemCode,
        string description,
        decimal quantityOrdered,
        string unitOfMeasure,
        Money unitPrice,
        decimal taxRatePercent)
        : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        LineNumber = lineNumber;
        ItemCode = itemCode;
        Description = description;
        QuantityOrdered = quantityOrdered;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        TaxRatePercent = taxRatePercent;
        QuantityReceived = 0m;
        QuantityInvoiced = 0m;
    }

    private PurchaseOrderLine() { }

    public Guid PurchaseOrderId { get; private set; }

    public int LineNumber { get; private set; }

    public string? ItemCode { get; private set; }

    public string Description { get; private set; } = null!;

    public decimal QuantityOrdered { get; private set; }

    public string UnitOfMeasure { get; private set; } = null!;

    public Money UnitPrice { get; private set; }

    public decimal TaxRatePercent { get; private set; }

    public decimal QuantityReceived { get; private set; }

    public decimal QuantityInvoiced { get; private set; }

    public decimal QuantityOutstanding => QuantityOrdered - QuantityReceived;

    public decimal QuantityAwaitingInvoice => QuantityReceived - QuantityInvoiced;

    public Money NetTotal => UnitPrice * QuantityOrdered;

    public Money TaxTotal => NetTotal * (TaxRatePercent / 100m);

    public Money GrossTotal => NetTotal + TaxTotal;

    public bool IsFullyReceived => QuantityReceived >= QuantityOrdered;

    internal static PurchaseOrderLine Create(
        Guid purchaseOrderId,
        int lineNumber,
        string? itemCode,
        string description,
        decimal quantityOrdered,
        string unitOfMeasure,
        Money unitPrice,
        decimal taxRatePercent) =>
        new(Guid.CreateVersion7(), purchaseOrderId, lineNumber, itemCode,
            description, quantityOrdered, unitOfMeasure, unitPrice, taxRatePercent);

    internal Result RecordReceipt(decimal quantity, decimal overReceiptTolerancePercent)
    {
        if (quantity <= 0)
        {
            return Result.Failure(PurchaseOrderErrors.ReceiptQuantityMustBePositive);
        }

        var maximum = QuantityOrdered * (1m + (overReceiptTolerancePercent / 100m));

        if (QuantityReceived + quantity > maximum)
        {
            return Result.Failure(PurchaseOrderErrors.OverReceipt);
        }

        QuantityReceived += quantity;

        return Result.Success();
    }

    internal Result RecordInvoiced(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(PurchaseOrderErrors.InvoiceQuantityMustBePositive);
        }

        if (QuantityInvoiced + quantity > QuantityReceived)
        {
            return Result.Failure(PurchaseOrderErrors.InvoicedExceedsReceived);
        }

        QuantityInvoiced += quantity;

        return Result.Success();
    }

    internal void ReverseReceipt(decimal quantity) =>
        QuantityReceived = Math.Max(0m, QuantityReceived - quantity);
}
