using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Organisation;
using P2P.Domain.Common;

namespace P2P.Application.Organisation.Queries.GetCostCentreLookup;

public sealed record GetCostCentreLookupQuery : IQuery<IReadOnlyList<CostCentreLookupDto>>;

internal sealed class GetCostCentreLookupQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCostCentreLookupQuery, IReadOnlyList<CostCentreLookupDto>>
{
    // Cost centres are reference data counted in tens or hundreds, but the
    // ceiling keeps a misconfigured organisation from flooding a dropdown.
    private const int MaxResults = 500;

    public async Task<Result<IReadOnlyList<CostCentreLookupDto>>> Handle(
        GetCostCentreLookupQuery request, CancellationToken ct)
    {
        var results = await context.CostCentres
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Take(MaxResults)
            .Select(c => new CostCentreLookupDto(c.Id, c.Code, c.Name))
            .ToListAsync(ct);

        return results;
    }
}
