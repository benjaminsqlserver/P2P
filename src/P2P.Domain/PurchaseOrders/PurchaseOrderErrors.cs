using P2P.Domain.Common;

namespace P2P.Domain.PurchaseOrders;

public static class PurchaseOrderErrors
{
    public static readonly Error RequisitionNotApproved =
        Error.Conflict("PurchaseOrder.RequisitionNotApproved",
            "A purchase order can only be raised from an approved requisition.");

    public static readonly Error CurrencyMismatch =
        Error.Conflict("PurchaseOrder.CurrencyMismatch",
            "The requisition currency does not match the vendor's default currency.");

    public static readonly Error OnlyDraftCanBeEdited =
        Error.Conflict("PurchaseOrder.OnlyDraftCanBeEdited",
            "An issued purchase order cannot be edited. Raise an amendment instead.");

    public static readonly Error AlreadyIssued =
        Error.Conflict("PurchaseOrder.AlreadyIssued",
            "This purchase order has already been issued.");

    public static readonly Error AtLeastOneLineRequired =
        Error.Conflict("PurchaseOrder.AtLeastOneLineRequired",
            "A purchase order must have at least one line before it is issued.");

    public static readonly Error LineQuantityMustBePositive =
        Error.Validation("PurchaseOrder.LineQuantityMustBePositive",
            "Line quantity must be greater than zero.");

    public static readonly Error NotReceivable =
        Error.Conflict("PurchaseOrder.NotReceivable",
            "Goods can only be received against an issued or partially "
            + "received purchase order.");

    public static readonly Error ReceiptQuantityMustBePositive =
        Error.Validation("PurchaseOrder.ReceiptQuantityMustBePositive",
            "Received quantity must be greater than zero.");

    public static readonly Error OverReceipt =
        Error.Conflict("PurchaseOrder.OverReceipt",
            "The received quantity exceeds the ordered quantity "
            + "beyond the permitted tolerance.");

    public static readonly Error InvoiceQuantityMustBePositive =
        Error.Validation("PurchaseOrder.InvoiceQuantityMustBePositive",
            "Invoiced quantity must be greater than zero.");

    public static readonly Error InvoicedExceedsReceived =
        Error.Conflict("PurchaseOrder.InvoicedExceedsReceived",
            "You cannot invoice more than has been received.");

    public static readonly Error CannotClose =
        Error.Conflict("PurchaseOrder.CannotClose",
            "A cancelled or already closed purchase order cannot be closed.");

    public static readonly Error CannotCancelAfterReceipt =
        Error.Conflict("PurchaseOrder.CannotCancelAfterReceipt",
            "A purchase order with recorded receipts cannot be cancelled.");

    public static readonly Error CancellationReasonRequired =
        Error.Validation("PurchaseOrder.CancellationReasonRequired",
            "A cancellation reason is required.");
}
