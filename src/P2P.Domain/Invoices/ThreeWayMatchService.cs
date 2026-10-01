using P2P.Domain.Common;
using P2P.Domain.PurchaseOrders;

namespace P2P.Domain.Invoices;

/// <summary>
/// Compares an invoice against its purchase order and the receipts recorded
/// against that order, producing a list of exceptions. Stateless and free of
/// I/O: everything it needs is passed in.
/// </summary>
public static class ThreeWayMatchService
{
    public static Result Match(
        Invoice invoice,
        PurchaseOrder purchaseOrder,
        MatchTolerance tolerance,
        DateTimeOffset nowUtc,
        bool vendorInvoiceNumberAlreadySeen)
    {
        if (invoice.PurchaseOrderId != purchaseOrder.Id)
        {
            return Result.Failure(InvoiceErrors.PurchaseOrderMismatch);
        }

        var exceptions = new List<MatchException>();

        // --- Guard 1: duplicate invoice number from the same vendor -------
        if (vendorInvoiceNumberAlreadySeen)
        {
            exceptions.Add(MatchException.Create(
                invoice.Id,
                null,
                MatchExceptionType.DuplicateInvoiceNumber,
                $"Invoice number '{invoice.VendorInvoiceNumber}' has already been "
                + "recorded against this vendor."));
        }

        // --- Guard 2: currency ------------------------------------------
        if (!string.Equals(invoice.Currency, purchaseOrder.Currency,
                StringComparison.Ordinal))
        {
            exceptions.Add(MatchException.Create(
                invoice.Id,
                null,
                MatchExceptionType.CurrencyMismatch,
                $"Invoice is in {invoice.Currency} but the purchase order is in "
                + $"{purchaseOrder.Currency}."));

            invoice.ApplyMatchResult(exceptions, nowUtc);

            return Result.Success();
        }

        // --- Line-by-line comparison -------------------------------------
        foreach (var invoiceLine in invoice.Lines)
        {
            if (invoiceLine.PurchaseOrderLineId is not { } poLineId)
            {
                exceptions.Add(MatchException.Create(
                    invoice.Id,
                    invoiceLine.Id,
                    MatchExceptionType.LineNotOnPurchaseOrder,
                    $"Line {invoiceLine.LineNumber} ('{invoiceLine.Description}') "
                    + "is not linked to any purchase order line."));

                continue;
            }

            var poLine = purchaseOrder.Lines.SingleOrDefault(l => l.Id == poLineId);

            if (poLine is null)
            {
                exceptions.Add(MatchException.Create(
                    invoice.Id,
                    invoiceLine.Id,
                    MatchExceptionType.LineNotOnPurchaseOrder,
                    $"Line {invoiceLine.LineNumber} references a purchase order "
                    + "line that does not exist."));

                continue;
            }

            // --- Quantity leg: invoiced must not exceed received ---------
            if (poLine.QuantityReceived == 0m)
            {
                exceptions.Add(MatchException.Create(
                    invoice.Id,
                    invoiceLine.Id,
                    MatchExceptionType.NoReceiptRecorded,
                    $"No goods receipt has been recorded for line "
                    + $"{poLine.LineNumber} ('{poLine.Description}').",
                    expectedValue: 0m,
                    actualValue: invoiceLine.Quantity));
            }
            else
            {
                var permittedQuantity = poLine.QuantityReceived
                    * (1m + (tolerance.QuantityPercent / 100m));
                var cumulativeInvoiced = poLine.QuantityInvoiced + invoiceLine.Quantity;

                if (cumulativeInvoiced > permittedQuantity)
                {
                    exceptions.Add(MatchException.Create(
                        invoice.Id,
                        invoiceLine.Id,
                        MatchExceptionType.QuantityOverBilled,
                        $"Line {invoiceLine.LineNumber}: billed quantity "
                        + $"{cumulativeInvoiced:N2} exceeds received quantity "
                        + $"{poLine.QuantityReceived:N2}.",
                        expectedValue: poLine.QuantityReceived,
                        actualValue: cumulativeInvoiced));
                }
            }

            // --- Price leg: invoice price must not exceed PO price -------
            if (invoiceLine.UnitPrice > poLine.UnitPrice)
            {
                var difference = Money.AbsoluteDifference(
                    invoiceLine.UnitPrice, poLine.UnitPrice);
                var lineVariance = difference * invoiceLine.Quantity;
                var permittedPrice = poLine.UnitPrice
                    * (1m + (tolerance.PricePercent / 100m));
                var breachesPercentage = invoiceLine.UnitPrice > permittedPrice;
                var breachesFloor = !tolerance.IsWithinAbsoluteFloor(lineVariance);

                if (breachesPercentage && breachesFloor)
                {
                    exceptions.Add(MatchException.Create(
                        invoice.Id,
                        invoiceLine.Id,
                        MatchExceptionType.PriceAboveTolerance,
                        $"Line {invoiceLine.LineNumber}: invoiced unit price "
                        + $"{invoiceLine.UnitPrice} exceeds the ordered price "
                        + $"{poLine.UnitPrice} beyond the "
                        + $"{tolerance.PricePercent:N2}% tolerance.",
                        expectedValue: poLine.UnitPrice.Amount,
                        actualValue: invoiceLine.UnitPrice.Amount));
                }
            }
        }

        invoice.ApplyMatchResult(exceptions, nowUtc);

        return Result.Success();
    }
}
