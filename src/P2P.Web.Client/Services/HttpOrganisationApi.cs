using System.Net.Http.Json;
using P2P.Contracts.Organisation;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IOrganisationApi"/>.</summary>
internal sealed class HttpOrganisationApi(HttpClient http) : IOrganisationApi
{
    public async Task<IReadOnlyList<CostCentreLookupDto>> GetCostCentreLookupAsync(
        CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<CostCentreLookupDto>>(
            "api/organisation/cost-centres/lookup", ct) ?? [];
}
