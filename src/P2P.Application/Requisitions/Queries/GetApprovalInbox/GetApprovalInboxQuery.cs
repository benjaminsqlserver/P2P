using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Application.Common;
using P2P.Contracts.Common;
using P2P.Contracts.Requisitions;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Queries.GetApprovalInbox;

public sealed record GetApprovalInboxQuery(GridQuery Query)
    : IQuery<PagedResult<RequisitionListItemDto>>;

internal sealed class GetApprovalInboxQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : IQueryHandler<GetApprovalInboxQuery, PagedResult<RequisitionListItemDto>>
{
    public async Task<Result<PagedResult<RequisitionListItemDto>>> Handle(
        GetApprovalInboxQuery request, CancellationToken ct)
    {
        var roles = currentUser.Roles.ToArray();
        var userId = currentUser.UserId;

        // A requisition is in my inbox when its earliest pending step
        // requires one of my roles (or is assigned to me by name), and
        // I am not the requester.
        var baseQuery =
            from r in context.Requisitions.AsNoTracking()
            where r.Status == RequisitionStatus.PendingApproval
                  && r.RequesterId != userId
            let pending = r.ApprovalSteps
                .Where(s => s.Status == ApprovalStepStatus.Pending)
                .OrderBy(s => s.Sequence)
                .FirstOrDefault()
            where pending != null
                  && (roles.Contains(pending.RequiredRole)
                      || pending.AssignedApproverId == userId)
            select new { Requisition = r, Pending = pending };

        if (!string.IsNullOrWhiteSpace(request.Query.Search))
        {
            var term = request.Query.Search.Trim();

            baseQuery = baseQuery.Where(x =>
                EF.Functions.Like(x.Requisition.Number, $"%{term}%") ||
                EF.Functions.Like(x.Requisition.Title, $"%{term}%"));
        }

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(x => x.Requisition.SubmittedAtUtc)
            .Skip(request.Query.Skip)
            .Take(request.Query.Top)
            .Select(x => new RequisitionListItemDto
            {
                Id = x.Requisition.Id,
                Number = x.Requisition.Number,
                Title = x.Requisition.Title,
                RequesterName = context.CostCentres
                    .Where(c => c.Id == x.Requisition.CostCentreId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? "—",
                CostCentreCode = context.CostCentres
                    .Where(c => c.Id == x.Requisition.CostCentreId)
                    .Select(c => c.Code)
                    .FirstOrDefault() ?? "—",
                Status = StatusMappings.ToDto(x.Requisition.Status),
                Currency = x.Requisition.Currency,
                Total = x.Requisition.Lines
                    .Sum(l => l.UnitPrice.Amount * l.Quantity),
                NeededBy = x.Requisition.NeededBy,
                SubmittedAtUtc = x.Requisition.SubmittedAtUtc,
                PendingStepSequence = x.Pending!.Sequence,
                PendingStepRole = x.Pending.RequiredRole
            })
            .ToListAsync(ct);

        // Age is computed after the query, from IClock: EF cannot translate
        // DateTimeOffset.UtcDateTime inside DateDiffDay, and handlers must
        // not read the system clock directly.
        var now = clock.UtcNow;

        return new PagedResult<RequisitionListItemDto>
        {
            Items = items
                .Select(i => i with
                {
                    AgeInDays = i.SubmittedAtUtc is { } submitted
                        ? (int)(now - submitted).TotalDays
                        : 0
                })
                .ToList(),
            TotalCount = total
        };
    }
}
