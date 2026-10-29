using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Application.Common;
using P2P.Contracts.Common;
using P2P.Contracts.Vendors;
using P2P.Domain.Common;
using P2P.Domain.Invoices;
using P2P.Domain.PurchaseOrders;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Queries.GetVendorList;

internal sealed class GetVendorListQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetVendorListQuery, PagedResult<VendorListItemDto>>
{
    public async Task<Result<PagedResult<VendorListItemDto>>> Handle(
        GetVendorListQuery request, CancellationToken ct)
    {
        var q = request.Query;

        var vendors = context.Vendors.AsNoTracking();

        if (q.Status is { } status)
        {
            // Convert outside the expression: inside it EF sends the int, not
            // the string the status column stores, and nothing matches.
            var domainStatus = (VendorStatus)(int)status;
            vendors = vendors.Where(v => v.Status == domainStatus);
        }

        if (!string.IsNullOrWhiteSpace(q.CountryCode))
        {
            vendors = vendors.Where(
                v => v.RemitToAddress.CountryCode == q.CountryCode);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();

            vendors = vendors.Where(v =>
                EF.Functions.Like(v.Code, $"%{term}%") ||
                EF.Functions.Like(v.LegalName, $"%{term}%") ||
                (v.TradingName != null &&
                 EF.Functions.Like(v.TradingName, $"%{term}%")));
        }

        var total = await vendors.CountAsync(ct);

        vendors = ApplyOrdering(vendors, q.OrderBy, q.Descending);

        var items = await vendors
            .Skip(q.Skip)
            .Take(q.Top)
            .Select(v => new VendorListItemDto
            {
                Id = v.Id,
                Code = v.Code,
                LegalName = v.LegalName,
                TradingName = v.TradingName,
                Status = StatusMappings.ToDto(v.Status),
                DefaultCurrency = v.DefaultCurrency,
                PaymentTermsDays = v.PaymentTermsDays,
                City = v.RemitToAddress.City,
                CountryCode = v.RemitToAddress.CountryCode,

                OpenPurchaseOrderCount = context.PurchaseOrders
                    .Count(p => p.VendorId == v.Id &&
                                (p.Status == PurchaseOrderStatus.Issued ||
                                 p.Status == PurchaseOrderStatus.PartiallyReceived)),

                OutstandingBalance = context.Invoices
                    .Where(i => i.VendorId == v.Id &&
                                (i.Status == InvoiceStatus.Approved ||
                                 i.Status == InvoiceStatus.PartiallyPaid))
                    .Sum(i => (decimal?)i.AmountPaid.Amount) ?? 0m
            })
            .ToListAsync(ct);

        return new PagedResult<VendorListItemDto>
        {
            Items = items,
            TotalCount = total
        };
    }

    private static IQueryable<Vendor> ApplyOrdering(
        IQueryable<Vendor> source, string? orderBy, bool descending) =>
        orderBy switch
        {
            nameof(VendorListItemDto.Code) => descending
                ? source.OrderByDescending(v => v.Code)
                : source.OrderBy(v => v.Code),

            nameof(VendorListItemDto.LegalName) => descending
                ? source.OrderByDescending(v => v.LegalName)
                : source.OrderBy(v => v.LegalName),

            nameof(VendorListItemDto.Status) => descending
                ? source.OrderByDescending(v => v.Status)
                : source.OrderBy(v => v.Status),

            nameof(VendorListItemDto.PaymentTermsDays) => descending
                ? source.OrderByDescending(v => v.PaymentTermsDays)
                : source.OrderBy(v => v.PaymentTermsDays),

            _ => source.OrderBy(v => v.Code)
        };
}
