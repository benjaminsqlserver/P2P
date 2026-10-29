using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.PurchaseOrders.Commands.IssuePurchaseOrder;

public sealed record IssuePurchaseOrderCommand(Guid PurchaseOrderId) : ICommand;

internal sealed class IssuePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    IClock clock)
    : ICommandHandler<IssuePurchaseOrderCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        IssuePurchaseOrderCommand command, CancellationToken ct)
    {
        var order = await context.PurchaseOrders
            .Include(p => p.Lines)
            .SingleOrDefaultAsync(p => p.Id == command.PurchaseOrderId, ct);

        if (order is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(PurchaseOrder), command.PurchaseOrderId));
        }

        var result = order.Issue(clock.UtcNow);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
