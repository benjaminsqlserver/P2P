using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Contracts.Audit;
using P2P.Contracts.Common;
using P2P.Infrastructure.Persistence;

namespace P2P.Infrastructure.Services;

internal sealed class AuditTrailReader(ApplicationDbContext context) : IAuditTrailReader
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<AuditEntryDto>> GetPageAsync(
        GridQuery query, CancellationToken ct)
    {
        var entries = context.AuditEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();

            entries = entries.Where(a =>
                EF.Functions.Like(a.TableName, $"%{term}%") ||
                EF.Functions.Like(a.EntityId, $"%{term}%"));
        }

        var total = await entries.CountAsync(ct);

        var rows = await ApplyOrdering(entries, query.OrderBy, query.Descending)
            .Skip(Math.Max(0, query.Skip))
            .Take(Math.Clamp(query.Top, 1, MaxPageSize))
            .Select(a => new AuditEntryDto
            {
                Id = a.Id,
                OccurredAtUtc = a.OccurredAtUtc,
                UserName = a.UserId.ToString(),
                TableName = a.TableName,
                EntityId = a.EntityId,
                Action = a.Action,
                ChangedColumns = a.ChangedColumns,
                OldValues = a.OldValues,
                NewValues = a.NewValues
            })
            .ToListAsync(ct);

        return new PagedResult<AuditEntryDto>
        {
            Items = rows,
            TotalCount = total
        };
    }

    private static IQueryable<AuditEntry> ApplyOrdering(
        IQueryable<AuditEntry> source, string? orderBy, bool descending) =>
        orderBy switch
        {
            nameof(AuditEntryDto.UserName) => descending
                ? source.OrderByDescending(a => a.UserId)
                : source.OrderBy(a => a.UserId),

            nameof(AuditEntryDto.TableName) => descending
                ? source.OrderByDescending(a => a.TableName)
                : source.OrderBy(a => a.TableName),

            nameof(AuditEntryDto.Action) => descending
                ? source.OrderByDescending(a => a.Action)
                : source.OrderBy(a => a.Action),

            nameof(AuditEntryDto.OccurredAtUtc) when !descending =>
                source.OrderBy(a => a.OccurredAtUtc),

            // Newest first unless the user asks otherwise.
            _ => source.OrderByDescending(a => a.OccurredAtUtc)
        };
}
