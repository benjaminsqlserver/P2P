using FluentAssertions;
using P2P.Domain.Common;
using P2P.Domain.Invoices;
using P2P.Domain.PurchaseOrders;
using P2P.Domain.Requisitions;
using P2P.Domain.Vendors;

namespace P2P.Domain.Tests;

public class ThreeWayMatchTests
{
    private static readonly DateOnly Today = new(2026, 8, 24);

    private static readonly DateTimeOffset Now =
        new(2026, 8, 24, 9, 0, 0, TimeSpan.Zero);

    private static readonly Address AnAddress = new(
        "12 Ligali Ayorinde Street", null, "Lagos", "Lagos", "106104", "NG");

    private static Vendor AnActiveVendor()
    {
        var vendor = Vendor.Create(
            "V-0001", "Acme Supplies Limited", "Acme", "TIN-1234567",
            AnAddress, "NGN", 30).Value;

        vendor.AddBankAccount(
            "Zenith Bank", "Acme Supplies Limited", "1234567890",
            "ZEIBNGLA", "NGN", makePrimary: true);
        vendor.Activate();

        return vendor;
    }

    private static (PurchaseOrder Order, PurchaseOrderLine Line) AnIssuedOrder(
        decimal quantity, decimal unitPrice)
    {
        var requisition = Requisition.Create(
            "REQ-2026-000001", Guid.CreateVersion7(), Guid.CreateVersion7(),
            "Laptops", null, Today.AddDays(14), "NGN", Today).Value;
        requisition.AddLine(null, "Laptop", quantity, "EA", unitPrice, null);
        requisition.Submit(
            [new ApprovalRouteStep(1, "DepartmentHead", Guid.CreateVersion7())], Now);
        var approver = requisition.ApprovalSteps.Single().AssignedApproverId!.Value;
        requisition.Approve(approver, null, Now);

        var order = PurchaseOrder.CreateFromRequisition(
            "PO-2026-000001", requisition, AnActiveVendor(), Guid.CreateVersion7(),
            Today, Today.AddDays(14), AnAddress, defaultTaxRatePercent: 7.5m).Value;
        order.Issue(Now);

        return (order, order.Lines.Single());
    }

    [Fact]
    public void A_perfectly_matching_invoice_produces_no_exceptions()
    {
        var (order, poLine) = AnIssuedOrder(quantity: 10m, unitPrice: 850_000m);
        order.ApplyReceipt(
            new Dictionary<Guid, decimal> { [poLine.Id] = 10m }, 0m);
        var invoice = Invoice.Create(
            "INV-2026-000001", "ACME-9912", order.VendorId, order.Id,
            Today, Today.AddDays(30), "NGN").Value;
        invoice.AddLine(poLine.Id, "Laptop", 10m, 850_000m, 7.5m);

        ThreeWayMatchService.Match(
            invoice, order, MatchTolerance.Default("NGN"), Now,
            vendorInvoiceNumberAlreadySeen: false);

        invoice.MatchExceptions.Should().BeEmpty();
        invoice.Status.Should().Be(InvoiceStatus.Matched);
    }

    [Fact]
    public void Billing_more_than_was_received_raises_a_quantity_exception()
    {
        var (order, poLine) = AnIssuedOrder(quantity: 10m, unitPrice: 850_000m);
        order.ApplyReceipt(
            new Dictionary<Guid, decimal> { [poLine.Id] = 6m }, 0m);
        var invoice = Invoice.Create(
            "INV-2026-000002", "ACME-9913", order.VendorId, order.Id,
            Today, Today.AddDays(30), "NGN").Value;
        invoice.AddLine(poLine.Id, "Laptop", 10m, 850_000m, 7.5m);

        ThreeWayMatchService.Match(
            invoice, order, MatchTolerance.Default("NGN"), Now, false);

        invoice.Status.Should().Be(InvoiceStatus.Exception);
        invoice.MatchExceptions.Should().ContainSingle()
            .Which.Type.Should().Be(MatchExceptionType.QuantityOverBilled);
    }

    [Fact]
    public void An_invoice_with_no_receipt_raises_a_no_receipt_exception()
    {
        var (order, poLine) = AnIssuedOrder(quantity: 10m, unitPrice: 850_000m);
        var invoice = Invoice.Create(
            "INV-2026-000003", "ACME-9914", order.VendorId, order.Id,
            Today, Today.AddDays(30), "NGN").Value;
        invoice.AddLine(poLine.Id, "Laptop", 10m, 850_000m, 7.5m);

        ThreeWayMatchService.Match(
            invoice, order, MatchTolerance.Default("NGN"), Now, false);

        invoice.MatchExceptions.Should().ContainSingle()
            .Which.Type.Should().Be(MatchExceptionType.NoReceiptRecorded);
    }

    [Fact]
    public void A_small_price_variance_within_the_absolute_floor_is_ignored()
    {
        var (order, poLine) = AnIssuedOrder(quantity: 2m, unitPrice: 2m);
        order.ApplyReceipt(new Dictionary<Guid, decimal> { [poLine.Id] = 2m }, 0m);
        var invoice = Invoice.Create(
            "INV-2026-000004", "ACME-9915", order.VendorId, order.Id,
            Today, Today.AddDays(30), "NGN").Value;

        // 25% above the ordered price, but only 1.00 of variance in total.
        invoice.AddLine(poLine.Id, "Laptop", 2m, 2.50m, 7.5m);

        ThreeWayMatchService.Match(
            invoice, order, MatchTolerance.Default("NGN"), Now, false);

        invoice.MatchExceptions.Should().BeEmpty();
    }

    [Fact]
    public void A_material_price_variance_raises_a_price_exception()
    {
        var (order, poLine) = AnIssuedOrder(quantity: 10m, unitPrice: 850_000m);
        order.ApplyReceipt(new Dictionary<Guid, decimal> { [poLine.Id] = 10m }, 0m);
        var invoice = Invoice.Create(
            "INV-2026-000005", "ACME-9916", order.VendorId, order.Id,
            Today, Today.AddDays(30), "NGN").Value;
        invoice.AddLine(poLine.Id, "Laptop", 10m, 920_000m, 7.5m);

        ThreeWayMatchService.Match(
            invoice, order, MatchTolerance.Default("NGN"), Now, false);

        invoice.Status.Should().Be(InvoiceStatus.Exception);
        invoice.MatchExceptions.Should().ContainSingle()
            .Which.Type.Should().Be(MatchExceptionType.PriceAboveTolerance);
    }

    [Fact]
    public void Resolving_the_last_exception_returns_the_invoice_to_matched()
    {
        var (order, poLine) = AnIssuedOrder(quantity: 10m, unitPrice: 850_000m);
        order.ApplyReceipt(new Dictionary<Guid, decimal> { [poLine.Id] = 10m }, 0m);
        var invoice = Invoice.Create(
            "INV-2026-000006", "ACME-9917", order.VendorId, order.Id,
            Today, Today.AddDays(30), "NGN").Value;
        invoice.AddLine(poLine.Id, "Laptop", 10m, 920_000m, 7.5m);
        ThreeWayMatchService.Match(
            invoice, order, MatchTolerance.Default("NGN"), Now, false);
        var exception = invoice.MatchExceptions.Single();

        invoice.ResolveException(
            exception.Id, Guid.CreateVersion7(),
            "Price increase agreed by email 2026-08-12.", Now);

        invoice.Status.Should().Be(InvoiceStatus.Matched);
        invoice.HasUnresolvedExceptions.Should().BeFalse();
    }
}
