using P2P.Domain.Common;

namespace P2P.Domain.Invoices;

public static class InvoiceErrors
{
    public static readonly Error VendorInvoiceNumberRequired =
        Error.Validation("Invoice.VendorInvoiceNumberRequired",
            "The vendor's invoice number is required.");

    public static readonly Error DueDateBeforeInvoiceDate =
        Error.Validation("Invoice.DueDateBeforeInvoiceDate",
            "The due date cannot precede the invoice date.");

    public static readonly Error NotEditable =
        Error.Conflict("Invoice.NotEditable",
            "This invoice can no longer be edited.");

    public static readonly Error LineQuantityMustBePositive =
        Error.Validation("Invoice.LineQuantityMustBePositive",
            "Invoice line quantity must be greater than zero.");

    public static readonly Error PurchaseOrderMismatch =
        Error.Conflict("Invoice.PurchaseOrderMismatch",
            "The supplied purchase order is not the one this invoice references.");

    public static readonly Error ResolutionRequired =
        Error.Validation("Invoice.ResolutionRequired",
            "A resolution note is required when clearing a match exception.");

    public static readonly Error OnlyMatchedCanBeApproved =
        Error.Conflict("Invoice.OnlyMatchedCanBeApproved",
            "Only a successfully matched invoice can be approved for payment.");

    public static readonly Error UnresolvedExceptions =
        Error.Conflict("Invoice.UnresolvedExceptions",
            "This invoice has unresolved match exceptions.");

    public static readonly Error CannotDispute =
        Error.Conflict("Invoice.CannotDispute",
            "A paid or cancelled invoice cannot be disputed.");

    public static readonly Error DisputeReasonRequired =
        Error.Validation("Invoice.DisputeReasonRequired",
            "A reason is required when disputing an invoice.");

    public static readonly Error NotPayable =
        Error.Conflict("Invoice.NotPayable",
            "This invoice is not approved for payment.");

    public static readonly Error PaymentAmountMustBePositive =
        Error.Validation("Invoice.PaymentAmountMustBePositive",
            "The payment amount must be greater than zero.");

    public static readonly Error OverPayment =
        Error.Conflict("Invoice.OverPayment",
            "The payment exceeds the amount outstanding on this invoice.");
}
