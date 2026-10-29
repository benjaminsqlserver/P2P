using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.PurchaseOrders;
using P2P.Domain.Common;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.PurchaseOrders.Queries.SearchOpenPurchaseOrders;

/// <summary>Issued or partially received orders, for the receiving clerk's picker.</summary>
public sealed record SearchOpenPurchaseOrdersQuery(string? Term)
    : IQuery<IReadOnlyList<PurchaseOrderLookupDto>>;

internal sealed class SearchOpenPurchaseOrdersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchOpenPurchaseOrdersQuery, IReadOnlyList<PurchaseOrderLookupDto>>
{
    private const int MaxResults = 30;

    public async Task<Result<IReadOnlyList<PurchaseOrderLookupDto>>> Handle(
        SearchOpenPurchaseOrdersQuery request, CancellationToken ct)
    {
        var query =
            from p in context.PurchaseOrders.AsNoTracking()
            join v in context.Vendors.AsNoTracking() on p.VendorId equals v.Id
            where p.Status == PurchaseOrderStatus.Issued
                  || p.Status == PurchaseOrderStatus.PartiallyReceived
            select new { Order = p, VendorName = v.LegalName };

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Order.Number, $"%{term}%") ||
                EF.Functions.Like(x.VendorName, $"%{term}%"));
        }

        var results = await query
            .OrderBy(x => x.Order.ExpectedDeliveryDate)
            .Take(MaxResults)
            .Select(x => new PurchaseOrderLookupDto(
                x.Order.Id, x.Order.Number, x.VendorName, x.Order.ExpectedDeliveryDate))
            .ToListAsync(ct);

        return results;
    }
}
