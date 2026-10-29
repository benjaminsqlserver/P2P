using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Common;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Commands.ApproveRequisition;

internal sealed class ApproveRequisitionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : ICommandHandler<ApproveRequisitionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        ApproveRequisitionCommand command, CancellationToken ct)
    {
        var requisition = await context.Requisitions
            .Include(r => r.Lines)
            .Include(r => r.ApprovalSteps)
            .SingleOrDefaultAsync(r => r.Id == command.RequisitionId, ct);

        if (requisition is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(Requisition), command.RequisitionId));
        }

        var pendingStep = requisition.ApprovalSteps
            .Where(s => s.Status == ApprovalStepStatus.Pending)
            .OrderBy(s => s.Sequence)
            .FirstOrDefault();

        if (pendingStep is not null
            && !currentUser.IsInRole(pendingStep.RequiredRole)
            && !currentUser.IsInRole(Roles.ProcurementAdministrator))
        {
            return Result.Failure<Unit>(RequisitionErrors.NotTheAssignedApprover);
        }

        var result = requisition.Approve(
            currentUser.UserId, command.Comment, clock.UtcNow);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
