using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Commands.UpdateVendor;

internal sealed class UpdateVendorCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateVendorCommand, Unit>
{
    public async Task<Result<Unit>> Handle(
        UpdateVendorCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var vendor = await context.Vendors
            .SingleOrDefaultAsync(v => v.Id == request.Id, ct);

        if (vendor is null)
        {
            return Result.Failure<Unit>(Error.NotFound(nameof(Vendor), request.Id));
        }

        var address = new Address(
            request.RemitToAddress.Line1,
            request.RemitToAddress.Line2,
            request.RemitToAddress.City,
            request.RemitToAddress.StateOrProvince,
            request.RemitToAddress.PostalCode,
            request.RemitToAddress.CountryCode.ToUpperInvariant());

        var result = vendor.UpdateDetails(
            request.LegalName,
            request.TradingName,
            request.TaxIdentifier,
            address,
            request.PaymentTermsDays);

        return result.IsFailure
            ? Result.Failure<Unit>(result.Error)
            : Result.Success(Unit.Value);
    }
}
