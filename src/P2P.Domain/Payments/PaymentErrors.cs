using P2P.Domain.Common;

namespace P2P.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error OnlyDraftCanBeEdited =
        Error.Conflict("Payment.OnlyDraftCanBeEdited",
            "Only a draft payment can be edited.");

    public static readonly Error AllocationMustBePositive =
        Error.Validation("Payment.AllocationMustBePositive",
            "An allocation amount must be greater than zero.");

    public static readonly Error DuplicateAllocation =
        Error.Conflict("Payment.DuplicateAllocation",
            "This invoice is already allocated on this payment.");

    public static readonly Error OnlyDraftCanBeAuthorised =
        Error.Conflict("Payment.OnlyDraftCanBeAuthorised",
            "Only a draft payment can be authorised.");

    public static readonly Error NothingAllocated =
        Error.Conflict("Payment.NothingAllocated",
            "A payment must allocate to at least one invoice.");

    public static readonly Error OnlyAuthorisedCanBeExecuted =
        Error.Conflict("Payment.OnlyAuthorisedCanBeExecuted",
            "Only an authorised payment can be marked as executed.");
}
