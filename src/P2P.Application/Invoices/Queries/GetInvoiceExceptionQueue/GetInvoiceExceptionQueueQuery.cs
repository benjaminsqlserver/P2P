using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Common;
using P2P.Contracts.Invoices;
using P2P.Domain.Common;
using P2P.Domain.Invoices;

namespace P2P.Application.Invoices.Queries.GetInvoiceExceptionQueue;

/// <summary>Invoices with at least one unresolved match exception.</summary>
public sealed record GetInvoiceExceptionQueueQuery(InvoiceExceptionQuery Query)
    : IQuery<PagedResult<InvoiceExceptionRowDto>>;

internal sealed class GetInvoiceExceptionQueueQueryHandler(
    IApplicationDbContext context,
    IClock clock)
    : IQueryHandler<GetInvoiceExceptionQueueQuery, PagedResult<InvoiceExceptionRowDto>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<InvoiceExceptionRowDto>>> Handle(
        GetInvoiceExceptionQueueQuery request, CancellationToken ct)
    {
        var q = request.Query;

        var invoices = context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Exception
                        && i.MatchExceptions.Any(e => !e.IsResolved));

        if (q.ExceptionType is { } type)
        {
            var domainType = (MatchExceptionType)(int)type;

            invoices = invoices.Where(i => i.MatchExceptions
                .Any(e => !e.IsResolved && e.Type == domainType));
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();

            invoices = invoices.Where(i =>
                EF.Functions.Like(i.InternalNumber, $"%{term}%") ||
                EF.Functions.Like(i.VendorInvoiceNumber, $"%{term}%"));
        }

        var total = await invoices.CountAsync(ct);

        invoices = ApplyOrdering(invoices, q.OrderBy, q.Descending);

        var rows = await invoices
            .Skip(Math.Max(0, q.Skip))
            .Take(Math.Clamp(q.Top, 1, MaxPageSize))
            .Select(i => new
            {
                i.Id,
                i.InternalNumber,
                i.VendorInvoiceNumber,
                VendorName = context.Vendors
                    .Where(v => v.Id == i.VendorId)
                    .Select(v => v.LegalName)
                    .FirstOrDefault(),
                PurchaseOrderNumber = context.PurchaseOrders
                    .Where(p => p.Id == i.PurchaseOrderId)
                    .Select(p => p.Number)
                    .FirstOrDefault(),
                ExceptionTypes = i.MatchExceptions
                    .Where(e => !e.IsResolved)
                    .Select(e => e.Type)
                    .ToList(),
                GrossTotal = i.Lines.Sum(l =>
                    l.UnitPrice.Amount * l.Quantity * (1m + (l.TaxRatePercent / 100m))),
                i.Currency,
                i.MatchedAtUtc
            })
            .ToListAsync(ct);

        var now = clock.UtcNow;

        var items = rows
            .Select(r => new InvoiceExceptionRowDto
            {
                InvoiceId = r.Id,
                InternalNumber = r.InternalNumber,
                VendorInvoiceNumber = r.VendorInvoiceNumber,
                VendorName = r.VendorName ?? "—",
                PurchaseOrderNumber = r.PurchaseOrderNumber ?? "—",
                ExceptionTypes = r.ExceptionTypes
                    .Select(t => (MatchExceptionTypeDto)(int)t)
                    .ToList(),
                GrossTotal = decimal.Round(r.GrossTotal, 2, MidpointRounding.ToEven),
                Currency = r.Currency,
                AgeInDays = r.MatchedAtUtc is { } matched
                    ? (int)(now - matched).TotalDays
                    : 0
            })
            .ToList();

        return new PagedResult<InvoiceExceptionRowDto>
        {
            Items = items,
            TotalCount = total
        };
    }

    private static IQueryable<Invoice> ApplyOrdering(
        IQueryable<Invoice> source, string? orderBy, bool descending) =>
        orderBy switch
        {
            nameof(InvoiceExceptionRowDto.InternalNumber) => descending
                ? source.OrderByDescending(i => i.InternalNumber)
                : source.OrderBy(i => i.InternalNumber),

            nameof(InvoiceExceptionRowDto.VendorInvoiceNumber) => descending
                ? source.OrderByDescending(i => i.VendorInvoiceNumber)
                : source.OrderBy(i => i.VendorInvoiceNumber),

            // Older exceptions have earlier match times, so "age descending"
            // is "matched ascending".
            nameof(InvoiceExceptionRowDto.AgeInDays) => descending
                ? source.OrderBy(i => i.MatchedAtUtc)
                : source.OrderByDescending(i => i.MatchedAtUtc),

            _ => source.OrderBy(i => i.MatchedAtUtc)
        };
}
