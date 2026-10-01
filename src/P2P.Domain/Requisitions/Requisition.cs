using P2P.Domain.Common;

namespace P2P.Domain.Requisitions;

public sealed class Requisition : AggregateRoot
{
    private readonly List<RequisitionLine> _lines = [];
    private readonly List<ApprovalStep> _approvalSteps = [];

    private Requisition(
        Guid id,
        string number,
        Guid requesterId,
        Guid costCentreId,
        string title,
        string? justification,
        DateOnly neededBy,
        string currency)
        : base(id)
    {
        Number = number;
        RequesterId = requesterId;
        CostCentreId = costCentreId;
        Title = title;
        Justification = justification;
        NeededBy = neededBy;
        Currency = currency;
        Status = RequisitionStatus.Draft;
    }

    private Requisition() { }

    public string Number { get; private set; } = null!;

    public Guid RequesterId { get; private set; }

    public Guid CostCentreId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Justification { get; private set; }

    public DateOnly NeededBy { get; private set; }

    public string Currency { get; private set; } = null!;

    public RequisitionStatus Status { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public DateTimeOffset? DecidedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public IReadOnlyCollection<RequisitionLine> Lines => _lines.AsReadOnly();

    public IReadOnlyCollection<ApprovalStep> ApprovalSteps =>
        _approvalSteps.AsReadOnly();

    public Money Total => _lines.Count == 0
        ? Money.Zero(Currency)
        : _lines.Aggregate(
            Money.Zero(Currency), (running, line) => running + line.LineTotal);

    public bool IsEditable => Status == RequisitionStatus.Draft;

    public static Result<Requisition> Create(
        string number,
        Guid requesterId,
        Guid costCentreId,
        string title,
        string? justification,
        DateOnly neededBy,
        string currency,
        DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Requisition>(RequisitionErrors.TitleRequired);
        }

        if (neededBy < today)
        {
            return Result.Failure<Requisition>(RequisitionErrors.NeededByInThePast);
        }

        var requisition = new Requisition(
            Guid.CreateVersion7(), number, requesterId, costCentreId,
            title.Trim(),
            string.IsNullOrWhiteSpace(justification) ? null : justification.Trim(),
            neededBy, currency.ToUpperInvariant());

        requisition.Raise(new RequisitionCreated(
            requisition.Id, requisition.Number, requesterId));

        return requisition;
    }

    public Result<RequisitionLine> AddLine(
        string? itemCode,
        string description,
        decimal quantity,
        string unitOfMeasure,
        decimal unitPrice,
        Guid? suggestedVendorId)
    {
        if (!IsEditable)
        {
            return Result.Failure<RequisitionLine>(RequisitionErrors.NotEditable);
        }

        var lineResult = RequisitionLine.Create(
            Id,
            _lines.Count + 1,
            itemCode,
            description,
            quantity,
            unitOfMeasure,
            new Money(unitPrice, Currency),
            suggestedVendorId);

        if (lineResult.IsFailure)
        {
            return lineResult;
        }

        _lines.Add(lineResult.Value);

        return lineResult;
    }

    public Result RemoveLine(Guid lineId)
    {
        if (!IsEditable)
        {
            return Result.Failure(RequisitionErrors.NotEditable);
        }

        var line = _lines.SingleOrDefault(l => l.Id == lineId);

        if (line is null)
        {
            return Result.Failure(Error.NotFound("RequisitionLine", lineId));
        }

        _lines.Remove(line);

        for (var i = 0; i < _lines.Count; i++)
        {
            _lines[i].Renumber(i + 1);
        }

        return Result.Success();
    }

    /// <summary>
    /// Moves the requisition into approval. The caller supplies the routing
    /// decision, because computing it requires knowledge of approval policy
    /// which lives outside this aggregate.
    /// </summary>
    public Result Submit(
        IReadOnlyList<ApprovalRouteStep> route,
        DateTimeOffset nowUtc)
    {
        if (Status != RequisitionStatus.Draft)
        {
            return Result.Failure(RequisitionErrors.OnlyDraftCanBeSubmitted);
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(RequisitionErrors.AtLeastOneLineRequired);
        }

        if (route.Count == 0)
        {
            return Result.Failure(RequisitionErrors.NoApprovalRouteResolved);
        }

        _approvalSteps.Clear();

        var sequence = 1;

        foreach (var step in route.OrderBy(s => s.Sequence))
        {
            _approvalSteps.Add(ApprovalStep.Create(
                Id, sequence++, step.RequiredRole, step.AssignedApproverId));
        }

        Status = RequisitionStatus.PendingApproval;
        SubmittedAtUtc = nowUtc;
        Raise(new RequisitionSubmitted(Id, Number, RequesterId, Total.Amount, Currency));

        return Result.Success();
    }

    public Result Approve(
        Guid approverId,
        string? comment,
        DateTimeOffset nowUtc)
    {
        if (Status != RequisitionStatus.PendingApproval)
        {
            return Result.Failure(RequisitionErrors.NotAwaitingApproval);
        }

        if (approverId == RequesterId)
        {
            return Result.Failure(RequisitionErrors.SelfApprovalForbidden);
        }

        var currentStep = _approvalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.Sequence)
            .FirstOrDefault();

        if (currentStep is null)
        {
            return Result.Failure(RequisitionErrors.NoPendingApprovalStep);
        }

        if (currentStep.AssignedApproverId is { } assigned && assigned != approverId)
        {
            return Result.Failure(RequisitionErrors.NotTheAssignedApprover);
        }

        currentStep.Approve(approverId, comment, nowUtc);
        Raise(new RequisitionStepApproved(
            Id, Number, currentStep.Sequence, approverId));

        var allDone = _approvalSteps.All(s => s.Status != ApprovalStepStatus.Pending);

        if (allDone)
        {
            Status = RequisitionStatus.Approved;
            DecidedAtUtc = nowUtc;
            Raise(new RequisitionApproved(Id, Number, RequesterId, Total.Amount, Currency));
        }

        return Result.Success();
    }

    public Result Reject(Guid approverId, string reason, DateTimeOffset nowUtc)
    {
        if (Status != RequisitionStatus.PendingApproval)
        {
            return Result.Failure(RequisitionErrors.NotAwaitingApproval);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(RequisitionErrors.RejectionReasonRequired);
        }

        var currentStep = _approvalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.Sequence)
            .FirstOrDefault();

        if (currentStep is null)
        {
            return Result.Failure(RequisitionErrors.NoPendingApprovalStep);
        }

        currentStep.Reject(approverId, reason.Trim(), nowUtc);

        foreach (var remaining in _approvalSteps
                     .Where(s => s.Status == ApprovalStepStatus.Pending))
        {
            remaining.Skip("Superseded by rejection at an earlier step.", nowUtc);
        }

        Status = RequisitionStatus.Rejected;
        RejectionReason = reason.Trim();
        DecidedAtUtc = nowUtc;
        Raise(new RequisitionRejected(Id, Number, RequesterId, RejectionReason));

        return Result.Success();
    }

    public Result Cancel(Guid actorId, DateTimeOffset nowUtc)
    {
        if (Status is RequisitionStatus.Converted or RequisitionStatus.Cancelled)
        {
            return Result.Failure(RequisitionErrors.CannotCancel);
        }

        Status = RequisitionStatus.Cancelled;
        DecidedAtUtc = nowUtc;
        Raise(new RequisitionCancelled(Id, Number, actorId));

        return Result.Success();
    }

    /// <summary>
    /// Called by the purchase order aggregate's creation path once a PO
    /// has been raised against this requisition.
    /// </summary>
    public Result MarkConverted()
    {
        if (Status != RequisitionStatus.Approved)
        {
            return Result.Failure(RequisitionErrors.OnlyApprovedCanBeConverted);
        }

        Status = RequisitionStatus.Converted;

        return Result.Success();
    }
}

/// <summary>
/// A single step in a resolved approval route. Produced by the approval
/// policy service and handed to <see cref="Requisition.Submit"/>.
/// </summary>
public sealed record ApprovalRouteStep(
    int Sequence,
    string RequiredRole,
    Guid? AssignedApproverId);
