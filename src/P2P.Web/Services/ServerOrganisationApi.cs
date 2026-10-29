using P2P.Application.Abstractions.Messaging;
using P2P.Application.Organisation.Queries.GetCostCentreLookup;
using P2P.Contracts.Organisation;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IOrganisationApi"/>.</summary>
internal sealed class ServerOrganisationApi(IDispatcher dispatcher) : IOrganisationApi
{
    public async Task<IReadOnlyList<CostCentreLookupDto>> GetCostCentreLookupAsync(
        CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetCostCentreLookupQuery(), ct);
        return result.IsSuccess ? result.Value : [];
    }
}
