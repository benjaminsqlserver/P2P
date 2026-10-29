using P2P.Domain.Common;

namespace P2P.Domain.Vendors;

public static class VendorErrors
{
    public static readonly Error CodeRequired =
        Error.Validation("Vendor.CodeRequired", "A vendor code is required.");

    public static readonly Error LegalNameRequired =
        Error.Validation("Vendor.LegalNameRequired", "A legal name is required.");

    public static readonly Error PaymentTermsOutOfRange =
        Error.Validation("Vendor.PaymentTermsOutOfRange",
            "Payment terms must be between 0 and 365 days.");

    public static readonly Error CodeAlreadyExists =
        Error.Conflict("Vendor.CodeAlreadyExists",
            "A vendor with this code already exists.");

    public static readonly Error CannotEditBlockedVendor =
        Error.Conflict("Vendor.CannotEditBlocked",
            "A blocked vendor cannot be edited. Unblock it first.");

    public static readonly Error CannotActivateBlockedVendor =
        Error.Conflict("Vendor.CannotActivateBlocked",
            "A blocked vendor cannot be activated.");

    public static readonly Error BankAccountRequiredForActivation =
        Error.Conflict("Vendor.BankAccountRequired",
            "A vendor must have at least one bank account before activation.");

    public static readonly Error HoldReasonRequired =
        Error.Validation("Vendor.HoldReasonRequired",
            "A reason must be supplied.");

    public static readonly Error AccountNumberRequired =
        Error.Validation("Vendor.AccountNumberRequired",
            "An account number is required.");

    public static readonly Error DuplicateBankAccount =
        Error.Conflict("Vendor.DuplicateBankAccount",
            "This account number is already recorded against the vendor.");

    public static readonly Error NotActive =
        Error.Conflict("Vendor.NotActive",
            "The vendor is not active and cannot receive purchase orders.");
}
