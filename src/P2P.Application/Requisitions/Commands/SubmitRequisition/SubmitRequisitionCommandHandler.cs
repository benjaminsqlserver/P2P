using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Common;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Commands.SubmitRequisition;

internal sealed class SubmitRequisitionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock,
    IApprovalRouter router)
    : ICommandHandler<SubmitRequisitionCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        SubmitRequisitionCommand command, CancellationToken ct)
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

        if (requisition.RequesterId != currentUser.UserId
            && !currentUser.IsInRole(Roles.ProcurementAdministrator))
        {
            return Result.Failure<Unit>(RequisitionErrors.NotEditable);
        }

        var route = await router.ResolveRouteAsync(
            requisition.CostCentreId,
            requisition.Total.Amount,
            requisition.Currency,
            ct);

        var result = requisition.Submit(route, clock.UtcNow);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
