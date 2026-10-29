using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Audit;
using P2P.Contracts.Common;
using P2P.Domain.Common;

namespace P2P.Application.Audit.Queries.GetAuditTrail;

public sealed record GetAuditTrailQuery(GridQuery Query) : IQuery<PagedResult<AuditEntryDto>>;

internal sealed class GetAuditTrailQueryHandler(
    IAuditTrailReader reader,
    ICurrentUser currentUser)
    : IQueryHandler<GetAuditTrailQuery, PagedResult<AuditEntryDto>>
{
    private static readonly Error Forbidden = new(
        "AuditTrail.Forbidden",
        "Only auditors and procurement administrators may read the audit trail.");

    public async Task<Result<PagedResult<AuditEntryDto>>> Handle(
        GetAuditTrailQuery request, CancellationToken ct)
    {
        // The audit blobs hold every mapped column, bank account numbers
        // included. The in-process API used by Blazor Server bypasses the
        // endpoint policy, so the role rule is enforced here as well.
        if (!currentUser.IsInRole(Roles.Auditor)
            && !currentUser.IsInRole(Roles.ProcurementAdministrator))
        {
            return Result.Failure<PagedResult<AuditEntryDto>>(Forbidden);
        }

        return await reader.GetPageAsync(request.Query, ct);
    }
}
