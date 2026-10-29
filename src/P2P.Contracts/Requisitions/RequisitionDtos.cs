namespace P2P.Contracts.Requisitions;

public enum RequisitionStatusDto
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Converted = 5
}

public sealed record RequisitionLineRequest
{
    public string? ItemCode { get; init; }
    public required string Description { get; init; }
    public required decimal Quantity { get; init; }
    public required string UnitOfMeasure { get; init; }
    public required decimal UnitPrice { get; init; }
    public Guid? SuggestedVendorId { get; init; }
}

public sealed record CreateRequisitionRequest
{
    public required Guid CostCentreId { get; init; }
    public required string Title { get; init; }
    public string? Justification { get; init; }
    public required DateOnly NeededBy { get; init; }
    public required string Currency { get; init; }
    public required IReadOnlyList<RequisitionLineRequest> Lines { get; init; }
}

public sealed record RequisitionListItemDto
{
    public required Guid Id { get; init; }
    public required string Number { get; init; }
    public required string Title { get; init; }
    public required string RequesterName { get; init; }
    public required string CostCentreCode { get; init; }
    public required RequisitionStatusDto Status { get; init; }
    public required decimal Total { get; init; }
    public required string Currency { get; init; }
    public required DateOnly NeededBy { get; init; }
    public DateTimeOffset? SubmittedAtUtc { get; init; }
    public int PendingStepSequence { get; init; }
    public string? PendingStepRole { get; init; }
    public int AgeInDays { get; init; }
}

public sealed record RequisitionLineDto
{
    public required Guid Id { get; init; }
    public required int LineNumber { get; init; }
    public string? ItemCode { get; init; }
    public required string Description { get; init; }
    public required decimal Quantity { get; init; }
    public required string UnitOfMeasure { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
}

public sealed record ApprovalStepDto
{
    public required Guid Id { get; init; }
    public required int Sequence { get; init; }
    public required string RequiredRole { get; init; }
    public string? ApproverName { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? ActionedAtUtc { get; init; }
    public string? Comment { get; init; }
}

public sealed record RequisitionDetailDto
{
    public required Guid Id { get; init; }
    public required string Number { get; init; }
    public required string Title { get; init; }
    public string? Justification { get; init; }
    public required Guid RequesterId { get; init; }
    public required string RequesterName { get; init; }
    public required Guid CostCentreId { get; init; }
    public required string CostCentreCode { get; init; }
    public required RequisitionStatusDto Status { get; init; }
    public required DateOnly NeededBy { get; init; }
    public required string Currency { get; init; }
    public required decimal Total { get; init; }
    public string? RejectionReason { get; init; }
    public required IReadOnlyList<RequisitionLineDto> Lines { get; init; }
    public required IReadOnlyList<ApprovalStepDto> ApprovalSteps { get; init; }
}

public sealed record RequisitionGridQuery : Common.GridQuery
{
    public RequisitionStatusDto? Status { get; init; }
    public Guid? RequesterId { get; init; }
    public Guid? CostCentreId { get; init; }
    public DateOnly? SubmittedFrom { get; init; }
    public DateOnly? SubmittedTo { get; init; }
    public bool OnlyMine { get; init; }
}
