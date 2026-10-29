using System.Net.Http.Json;
using P2P.Contracts.Audit;
using P2P.Contracts.Common;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IAuditApi"/>.</summary>
internal sealed class HttpAuditApi(HttpClient http) : IAuditApi
{
    public async Task<PagedResult<AuditEntryDto>> GetEntriesAsync(
        GridQuery query, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/audit/search", query, ct);

        if (!response.IsSuccessStatusCode)
        {
            return PagedResult<AuditEntryDto>.Empty();
        }

        return await response.Content
            .ReadFromJsonAsync<PagedResult<AuditEntryDto>>(ct)
            ?? PagedResult<AuditEntryDto>.Empty();
    }
}
