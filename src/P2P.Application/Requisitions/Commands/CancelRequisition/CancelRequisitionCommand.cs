using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Common;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Commands.CancelRequisition;

public sealed record CancelRequisitionCommand(Guid RequisitionId) : ICommand;

internal sealed class CancelRequisitionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : ICommandHandler<CancelRequisitionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        CancelRequisitionCommand command, CancellationToken ct)
    {
        var requisition = await context.Requisitions
            .SingleOrDefaultAsync(r => r.Id == command.RequisitionId, ct);

        if (requisition is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(Requisition), command.RequisitionId));
        }

        // Same rule as submission: only the requester or an administrator.
        if (requisition.RequesterId != currentUser.UserId
            && !currentUser.IsInRole(Roles.ProcurementAdministrator))
        {
            return Result.Failure<Unit>(RequisitionErrors.CannotCancel);
        }

        var result = requisition.Cancel(currentUser.UserId, clock.UtcNow);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
