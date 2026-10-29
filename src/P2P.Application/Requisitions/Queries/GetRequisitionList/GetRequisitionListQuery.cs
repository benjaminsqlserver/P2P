using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Application.Common;
using P2P.Contracts.Common;
using P2P.Contracts.Requisitions;
using P2P.Domain.Common;
using P2P.Domain.Requisitions;

namespace P2P.Application.Requisitions.Queries.GetRequisitionList;

public sealed record GetRequisitionListQuery(RequisitionGridQuery Query)
    : IQuery<PagedResult<RequisitionListItemDto>>;

internal sealed class GetRequisitionListQueryHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    IClock clock)
    : IQueryHandler<GetRequisitionListQuery, PagedResult<RequisitionListItemDto>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<RequisitionListItemDto>>> Handle(
        GetRequisitionListQuery request, CancellationToken ct)
    {
        var q = request.Query;

        var requisitions = context.Requisitions.AsNoTracking();

        if (q.Status is { } status)
        {
            // Convert outside the expression: inside it EF sends the int, not
            // the string the status column stores, and nothing matches.
            var domainStatus = (RequisitionStatus)(int)status;
            requisitions = requisitions.Where(r => r.Status == domainStatus);
        }

        if (q.OnlyMine)
        {
            var userId = currentUser.UserId;
            requisitions = requisitions.Where(r => r.RequesterId == userId);
        }
        else if (q.RequesterId is { } requesterId)
        {
            requisitions = requisitions.Where(r => r.RequesterId == requesterId);
        }

        if (q.CostCentreId is { } costCentreId)
        {
            requisitions = requisitions.Where(r => r.CostCentreId == costCentreId);
        }

        if (q.SubmittedFrom is { } from)
        {
            var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            requisitions = requisitions.Where(r => r.SubmittedAtUtc >= fromUtc);
        }

        if (q.SubmittedTo is { } to)
        {
            var toUtc = new DateTimeOffset(
                to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            requisitions = requisitions.Where(r => r.SubmittedAtUtc < toUtc);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();

            requisitions = requisitions.Where(r =>
                EF.Functions.Like(r.Number, $"%{term}%") ||
                EF.Functions.Like(r.Title, $"%{term}%"));
        }

        var total = await requisitions.CountAsync(ct);

        requisitions = ApplyOrdering(requisitions, q.OrderBy, q.Descending);

        var items = await requisitions
            .Skip(Math.Max(0, q.Skip))
            .Take(Math.Clamp(q.Top, 1, MaxPageSize))
            .Select(r => new RequisitionListItemDto
            {
                Id = r.Id,
                Number = r.Number,
                Title = r.Title,
                // There is no user directory in the model yet; the requester
                // is identified by id only.
                RequesterName = "—",
                CostCentreCode = context.CostCentres
                    .Where(c => c.Id == r.CostCentreId)
                    .Select(c => c.Code)
                    .FirstOrDefault() ?? "—",
                Status = StatusMappings.ToDto(r.Status),
                Currency = r.Currency,
                Total = r.Lines.Sum(l => l.UnitPrice.Amount * l.Quantity),
                NeededBy = r.NeededBy,
                SubmittedAtUtc = r.SubmittedAtUtc
            })
            .ToListAsync(ct);

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

    private static IQueryable<Requisition> ApplyOrdering(
        IQueryable<Requisition> source, string? orderBy, bool descending) =>
        orderBy switch
        {
            nameof(RequisitionListItemDto.Number) => descending
                ? source.OrderByDescending(r => r.Number)
                : source.OrderBy(r => r.Number),

            nameof(RequisitionListItemDto.Title) => descending
                ? source.OrderByDescending(r => r.Title)
                : source.OrderBy(r => r.Title),

            nameof(RequisitionListItemDto.NeededBy) => descending
                ? source.OrderByDescending(r => r.NeededBy)
                : source.OrderBy(r => r.NeededBy),

            nameof(RequisitionListItemDto.Status) => descending
                ? source.OrderByDescending(r => r.Status)
                : source.OrderBy(r => r.Status),

            nameof(RequisitionListItemDto.SubmittedAtUtc) => descending
                ? source.OrderByDescending(r => r.SubmittedAtUtc)
                : source.OrderBy(r => r.SubmittedAtUtc),

            _ => source.OrderByDescending(r => r.Number)
        };
}
