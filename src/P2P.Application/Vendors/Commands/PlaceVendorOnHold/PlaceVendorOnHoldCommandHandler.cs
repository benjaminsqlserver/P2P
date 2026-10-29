using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Commands.PlaceVendorOnHold;

internal sealed class PlaceVendorOnHoldCommandHandler(IApplicationDbContext context)
    : ICommandHandler<PlaceVendorOnHoldCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        PlaceVendorOnHoldCommand command, CancellationToken ct)
    {
        var vendor = await context.Vendors
            .SingleOrDefaultAsync(v => v.Id == command.VendorId, ct);

        if (vendor is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(Vendor), command.VendorId));
        }

        var result = vendor.PlaceOnHold(command.Reason);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
