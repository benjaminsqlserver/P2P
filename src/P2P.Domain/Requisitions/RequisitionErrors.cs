using P2P.Domain.Common;

namespace P2P.Domain.Requisitions;

public static class RequisitionErrors
{
    public static readonly Error TitleRequired =
        Error.Validation("Requisition.TitleRequired", "A title is required.");

    public static readonly Error NeededByInThePast =
        Error.Validation("Requisition.NeededByInThePast",
            "The needed-by date cannot be in the past.");

    public static readonly Error NotEditable =
        Error.Conflict("Requisition.NotEditable",
            "Only a draft requisition can be edited.");

    public static readonly Error LineDescriptionRequired =
        Error.Validation("Requisition.LineDescriptionRequired",
            "Each line must have a description.");

    public static readonly Error LineQuantityMustBePositive =
        Error.Validation("Requisition.LineQuantityMustBePositive",
            "Line quantity must be greater than zero.");

    public static readonly Error LinePriceMustNotBeNegative =
        Error.Validation("Requisition.LinePriceMustNotBeNegative",
            "Line unit price cannot be negative.");

    public static readonly Error AtLeastOneLineRequired =
        Error.Conflict("Requisition.AtLeastOneLineRequired",
            "A requisition must have at least one line before submission.");

    public static readonly Error OnlyDraftCanBeSubmitted =
        Error.Conflict("Requisition.OnlyDraftCanBeSubmitted",
            "Only a draft requisition can be submitted for approval.");

    public static readonly Error NoApprovalRouteResolved =
        Error.Conflict("Requisition.NoApprovalRoute",
            "No approval route could be resolved for this requisition. "
            + "Check the approval policy configuration.");

    public static readonly Error NotAwaitingApproval =
        Error.Conflict("Requisition.NotAwaitingApproval",
            "This requisition is not awaiting approval.");

    public static readonly Error SelfApprovalForbidden =
        Error.Conflict("Requisition.SelfApprovalForbidden",
            "You cannot approve a requisition that you raised.");

    public static readonly Error NoPendingApprovalStep =
        Error.Conflict("Requisition.NoPendingStep",
            "There is no pending approval step to action.");

    public static readonly Error NotTheAssignedApprover =
        Error.Conflict("Requisition.NotTheAssignedApprover",
            "This approval step is assigned to a different approver.");

    public static readonly Error RejectionReasonRequired =
        Error.Validation("Requisition.RejectionReasonRequired",
            "A reason must be supplied when rejecting.");

    public static readonly Error CannotCancel =
        Error.Conflict("Requisition.CannotCancel",
            "A converted or already cancelled requisition cannot be cancelled.");

    public static readonly Error OnlyApprovedCanBeConverted =
        Error.Conflict("Requisition.OnlyApprovedCanBeConverted",
            "Only an approved requisition can be converted to a purchase order.");
}
