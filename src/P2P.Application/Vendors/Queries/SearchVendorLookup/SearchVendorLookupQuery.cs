using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Vendors;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Queries.SearchVendorLookup;

public sealed record SearchVendorLookupQuery(string? Term)
    : IQuery<IReadOnlyList<VendorLookupDto>>;

internal sealed class SearchVendorLookupQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchVendorLookupQuery, IReadOnlyList<VendorLookupDto>>
{
    private const int MaxResults = 30;

    public async Task<Result<IReadOnlyList<VendorLookupDto>>> Handle(
        SearchVendorLookupQuery request, CancellationToken ct)
    {
        var query = context.Vendors
            .AsNoTracking()
            .Where(v => v.Status == VendorStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var term = request.Term.Trim();

            query = query.Where(v =>
                EF.Functions.Like(v.Code, $"{term}%") ||
                EF.Functions.Like(v.LegalName, $"%{term}%"));
        }

        var results = await query
            .OrderBy(v => v.LegalName)
            .Take(MaxResults)
            .Select(v => new VendorLookupDto(
                v.Id, v.Code, v.LegalName, v.DefaultCurrency))
            .ToListAsync(ct);

        return results;
    }
}
