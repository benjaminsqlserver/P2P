using P2P.Contracts.Common;

namespace P2P.Contracts.Audit;

public interface IAuditApi
{
    Task<PagedResult<AuditEntryDto>> GetEntriesAsync(
        GridQuery query, CancellationToken ct = default);
}
