using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Commands.RejectRequisition;

public sealed record RejectRequisitionCommand(Guid RequisitionId, string Reason)
    : ICommand;

internal sealed class RejectRequisitionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : ICommandHandler<RejectRequisitionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        RejectRequisitionCommand command, CancellationToken ct)
    {
        var requisition = await context.Requisitions
            .Include(r => r.ApprovalSteps)
            .SingleOrDefaultAsync(r => r.Id == command.RequisitionId, ct);

        if (requisition is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(Requisition), command.RequisitionId));
        }

        var result = requisition.Reject(
            currentUser.UserId, command.Reason, clock.UtcNow);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
