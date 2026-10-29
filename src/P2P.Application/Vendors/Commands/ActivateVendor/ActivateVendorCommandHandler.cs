using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Commands.ActivateVendor;

internal sealed class ActivateVendorCommandHandler(IApplicationDbContext context)
    : ICommandHandler<ActivateVendorCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        ActivateVendorCommand command, CancellationToken ct)
    {
        var vendor = await context.Vendors
            .Include(v => v.BankAccounts)
            .SingleOrDefaultAsync(v => v.Id == command.VendorId, ct);

        if (vendor is null)
        {
            return Result.Failure<Unit>(
                Error.NotFound(nameof(Vendor), command.VendorId));
        }

        var result = vendor.Activate();

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
