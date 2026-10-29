using P2P.Contracts.Audit;
using P2P.Contracts.Common;

namespace P2P.Application.Abstractions;

/// <summary>
/// Read access to the audit trail. The audit table belongs to
/// Infrastructure (it is written by an EF Core interceptor), so it is not
/// on <see cref="IApplicationDbContext"/>.
/// </summary>
public interface IAuditTrailReader
{
    Task<PagedResult<AuditEntryDto>> GetPageAsync(GridQuery query, CancellationToken ct);
}
