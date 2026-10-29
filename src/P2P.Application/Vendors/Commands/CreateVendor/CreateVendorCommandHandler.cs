using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Commands.CreateVendor;

internal sealed class CreateVendorCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateVendorCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateVendorCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var code = request.Code.Trim().ToUpperInvariant();

        var codeInUse = await context.Vendors
            .AsNoTracking()
            .AnyAsync(v => v.Code == code, ct);

        if (codeInUse)
        {
            return Result.Failure<Guid>(VendorErrors.CodeAlreadyExists);
        }

        var address = new Address(
            request.RemitToAddress.Line1,
            request.RemitToAddress.Line2,
            request.RemitToAddress.City,
            request.RemitToAddress.StateOrProvince,
            request.RemitToAddress.PostalCode,
            request.RemitToAddress.CountryCode.ToUpperInvariant());

        var vendorResult = Vendor.Create(
            code,
            request.LegalName,
            request.TradingName,
            request.TaxIdentifier,
            address,
            request.DefaultCurrency,
            request.PaymentTermsDays);

        if (vendorResult.IsFailure)
        {
            return Result.Failure<Guid>(vendorResult.Error);
        }

        context.Vendors.Add(vendorResult.Value);

        return vendorResult.Value.Id;
    }
}
