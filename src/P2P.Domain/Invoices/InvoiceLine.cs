using P2P.Domain.Common;

namespace P2P.Domain.Invoices;

public sealed class InvoiceLine : Entity
{
    private InvoiceLine(
        Guid id,
        Guid invoiceId,
        int lineNumber,
        Guid? purchaseOrderLineId,
        string description,
        decimal quantity,
        Money unitPrice,
        decimal taxRatePercent)
        : base(id)
    {
        InvoiceId = invoiceId;
        LineNumber = lineNumber;
        PurchaseOrderLineId = purchaseOrderLineId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRatePercent = taxRatePercent;
    }

    private InvoiceLine() { }

    public Guid InvoiceId { get; private set; }

    public int LineNumber { get; private set; }

    public Guid? PurchaseOrderLineId { get; private set; }

    public string Description { get; private set; } = null!;

    public decimal Quantity { get; private set; }

    public Money UnitPrice { get; private set; }

    public decimal TaxRatePercent { get; private set; }

    public Money NetTotal => UnitPrice * Quantity;

    public Money TaxTotal => NetTotal * (TaxRatePercent / 100m);

    public Money GrossTotal => NetTotal + TaxTotal;

    internal static InvoiceLine Create(
        Guid invoiceId,
        int lineNumber,
        Guid? purchaseOrderLineId,
        string description,
        decimal quantity,
        Money unitPrice,
        decimal taxRatePercent) =>
        new(Guid.CreateVersion7(), invoiceId, lineNumber, purchaseOrderLineId,
            description, quantity, unitPrice, taxRatePercent);
}
