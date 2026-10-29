using P2P.Application.Abstractions.Messaging;
using P2P.Application.Audit.Queries.GetAuditTrail;
using P2P.Contracts.Audit;
using P2P.Contracts.Common;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IAuditApi"/>.</summary>
internal sealed class ServerAuditApi(IDispatcher dispatcher) : IAuditApi
{
    public async Task<PagedResult<AuditEntryDto>> GetEntriesAsync(
        GridQuery query, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetAuditTrailQuery(query), ct);

        return result.IsSuccess
            ? result.Value
            : PagedResult<AuditEntryDto>.Empty();
    }
}
