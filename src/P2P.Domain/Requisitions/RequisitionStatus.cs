namespace P2P.Domain.Requisitions;

public enum RequisitionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Converted = 5
}

public enum ApprovalStepStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3
}
