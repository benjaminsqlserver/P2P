using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Application.Common;
using P2P.Contracts.Vendors;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Queries.GetVendorById;

internal sealed class GetVendorByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetVendorByIdQuery, VendorDetailDto>
{
    public async Task<Result<VendorDetailDto>> Handle(
        GetVendorByIdQuery request, CancellationToken ct)
    {
        var dto = await context.Vendors
            .AsNoTracking()
            .Where(v => v.Id == request.VendorId)
            .Select(v => new VendorDetailDto
            {
                Id = v.Id,
                Code = v.Code,
                LegalName = v.LegalName,
                TradingName = v.TradingName,
                TaxIdentifier = v.TaxIdentifier,
                DefaultCurrency = v.DefaultCurrency,
                PaymentTermsDays = v.PaymentTermsDays,
                Status = StatusMappings.ToDto(v.Status),
                HoldReason = v.HoldReason,
                RemitToAddress = new AddressDto
                {
                    Line1 = v.RemitToAddress.Line1,
                    Line2 = v.RemitToAddress.Line2,
                    City = v.RemitToAddress.City,
                    StateOrProvince = v.RemitToAddress.StateOrProvince,
                    PostalCode = v.RemitToAddress.PostalCode,
                    CountryCode = v.RemitToAddress.CountryCode
                },
                BankAccounts = v.BankAccounts
                    .Select(b => new BankAccountDto
                    {
                        Id = b.Id,
                        BankName = b.BankName,
                        AccountName = b.AccountName,
                        AccountNumber = b.AccountNumber,
                        SortCodeOrSwift = b.SortCodeOrSwift,
                        Currency = b.Currency,
                        IsPrimary = b.IsPrimary
                    })
                    .OrderByDescending(b => b.IsPrimary)
                    .ThenBy(b => b.BankName)
                    .ToList()
            })
            .SingleOrDefaultAsync(ct);

        return dto is null
            ? Result.Failure<VendorDetailDto>(
                Error.NotFound(nameof(Vendor), request.VendorId))
            : dto;
    }
}
