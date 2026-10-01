using P2P.Domain.Common;

namespace P2P.Domain.Requisitions;

public sealed class ApprovalStep : Entity
{
    private ApprovalStep(
        Guid id,
        Guid requisitionId,
        int sequence,
        string requiredRole,
        Guid? assignedApproverId)
        : base(id)
    {
        RequisitionId = requisitionId;
        Sequence = sequence;
        RequiredRole = requiredRole;
        AssignedApproverId = assignedApproverId;
        Status = ApprovalStepStatus.Pending;
    }

    private ApprovalStep() { }

    public Guid RequisitionId { get; private set; }

    public int Sequence { get; private set; }

    public string RequiredRole { get; private set; } = null!;

    public Guid? AssignedApproverId { get; private set; }

    public ApprovalStepStatus Status { get; private set; }

    public Guid? ActionedByUserId { get; private set; }

    public DateTimeOffset? ActionedAtUtc { get; private set; }

    public string? Comment { get; private set; }

    internal static ApprovalStep Create(
        Guid requisitionId, int sequence, string requiredRole, Guid? assignedApproverId) =>
        new(Guid.CreateVersion7(), requisitionId, sequence, requiredRole, assignedApproverId);

    internal void Approve(Guid approverId, string? comment, DateTimeOffset whenUtc)
    {
        Status = ApprovalStepStatus.Approved;
        ActionedByUserId = approverId;
        ActionedAtUtc = whenUtc;
        Comment = comment;
    }

    internal void Reject(Guid approverId, string reason, DateTimeOffset whenUtc)
    {
        Status = ApprovalStepStatus.Rejected;
        ActionedByUserId = approverId;
        ActionedAtUtc = whenUtc;
        Comment = reason;
    }

    internal void Skip(string reason, DateTimeOffset whenUtc)
    {
        Status = ApprovalStepStatus.Skipped;
        ActionedAtUtc = whenUtc;
        Comment = reason;
    }
}
