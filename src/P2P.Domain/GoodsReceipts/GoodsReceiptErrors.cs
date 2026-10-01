using P2P.Domain.Common;

namespace P2P.Domain.GoodsReceipts;

public static class GoodsReceiptErrors
{
    public static readonly Error ReceivedDateInTheFuture =
        Error.Validation("GoodsReceipt.ReceivedDateInTheFuture",
            "The receipt date cannot be in the future.");

    public static readonly Error QuantitiesMustNotBeNegative =
        Error.Validation("GoodsReceipt.NegativeQuantity",
            "Quantities cannot be negative.");

    public static readonly Error NothingRecorded =
        Error.Validation("GoodsReceipt.NothingRecorded",
            "A receipt line must record either an accepted or a rejected quantity.");

    public static readonly Error RejectionReasonRequired =
        Error.Validation("GoodsReceipt.RejectionReasonRequired",
            "A reason is required when rejecting delivered goods.");

    public static readonly Error DuplicateLine =
        Error.Conflict("GoodsReceipt.DuplicateLine",
            "This purchase order line already appears on the receipt.");

    public static readonly Error AtLeastOneLineRequired =
        Error.Conflict("GoodsReceipt.AtLeastOneLineRequired",
            "A goods receipt must record at least one line.");

    public static readonly Error AlreadyReversed =
        Error.Conflict("GoodsReceipt.AlreadyReversed",
            "This goods receipt has already been reversed.");

    public static readonly Error ReversalReasonRequired =
        Error.Validation("GoodsReceipt.ReversalReasonRequired",
            "A reason is required to reverse a goods receipt.");
}
