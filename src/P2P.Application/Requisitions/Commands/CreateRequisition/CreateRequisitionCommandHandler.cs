using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Organisation;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Commands.CreateRequisition;

internal sealed class CreateRequisitionCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock,
    INumberSequence numbers)
    : ICommandHandler<CreateRequisitionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateRequisitionCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var costCentreExists = await context.CostCentres
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CostCentreId && c.IsActive, ct);

        if (!costCentreExists)
        {
            return Result.Failure<Guid>(
                Error.NotFound(nameof(CostCentre), request.CostCentreId));
        }

        var number = await numbers.NextAsync("REQ", ct);

        var requisitionResult = Requisition.Create(
            number,
            currentUser.UserId,
            request.CostCentreId,
            request.Title,
            request.Justification,
            request.NeededBy,
            request.Currency,
            clock.Today);

        if (requisitionResult.IsFailure)
        {
            return Result.Failure<Guid>(requisitionResult.Error);
        }

        var requisition = requisitionResult.Value;

        foreach (var line in request.Lines)
        {
            var lineResult = requisition.AddLine(
                line.ItemCode,
                line.Description,
                line.Quantity,
                line.UnitOfMeasure,
                line.UnitPrice,
                line.SuggestedVendorId);

            if (lineResult.IsFailure)
            {
                return Result.Failure<Guid>(lineResult.Error);
            }
        }

        context.Requisitions.Add(requisition);

        return requisition.Id;
    }
}
