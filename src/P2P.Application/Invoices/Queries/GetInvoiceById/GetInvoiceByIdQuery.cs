using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.Invoices;
using P2P.Domain.Common;
using P2P.Domain.Invoices;

namespace P2P.Application.Invoices.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDetailDto>;

internal sealed class GetInvoiceByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetInvoiceByIdQuery, InvoiceDetailDto>
{
    public async Task<Result<InvoiceDetailDto>> Handle(
        GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Lines)
            .Include(i => i.MatchExceptions)
            .SingleOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice is null)
        {
            return Result.Failure<InvoiceDetailDto>(
                Error.NotFound(nameof(Invoice), request.InvoiceId));
        }

        var order = await context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Lines)
            .SingleOrDefaultAsync(p => p.Id == invoice.PurchaseOrderId, ct);

        var vendorName = await context.Vendors
            .AsNoTracking()
            .Where(v => v.Id == invoice.VendorId)
            .Select(v => v.LegalName)
            .FirstOrDefaultAsync(ct) ?? "—";

        var poLines = order?.Lines.ToDictionary(l => l.Id) ?? [];

        return new InvoiceDetailDto
        {
            Id = invoice.Id,
            InternalNumber = invoice.InternalNumber,
            VendorInvoiceNumber = invoice.VendorInvoiceNumber,
            VendorId = invoice.VendorId,
            VendorName = vendorName,
            PurchaseOrderId = invoice.PurchaseOrderId,
            PurchaseOrderNumber = order?.Number ?? "—",
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Currency = invoice.Currency,
            Status = (InvoiceStatusDto)(int)invoice.Status,
            NetTotal = invoice.NetTotal.Amount,
            TaxTotal = invoice.TaxTotal.Amount,
            GrossTotal = invoice.GrossTotal.Amount,
            AmountPaid = invoice.AmountPaid.Amount,
            Lines = invoice.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new InvoiceLineDto
                {
                    Id = l.Id,
                    LineNumber = l.LineNumber,
                    PurchaseOrderLineId = l.PurchaseOrderLineId,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice.Amount,
                    TaxRatePercent = l.TaxRatePercent,
                    NetTotal = l.NetTotal.Amount
                })
                .ToList(),
            MatchExceptions = invoice.MatchExceptions
                .OrderBy(e => e.IsResolved)
                .ThenBy(e => e.Type)
                .Select(e => new MatchExceptionDto
                {
                    Id = e.Id,
                    InvoiceLineId = e.InvoiceLineId,
                    Type = (MatchExceptionTypeDto)(int)e.Type,
                    Description = e.Description,
                    ExpectedValue = e.ExpectedValue,
                    ActualValue = e.ActualValue,
                    IsResolved = e.IsResolved,
                    Resolution = e.Resolution,
                    ResolvedAtUtc = e.ResolvedAtUtc
                })
                .ToList(),
            Comparison = invoice.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l =>
                {
                    var poLine = l.PurchaseOrderLineId is { } id
                                 && poLines.TryGetValue(id, out var match)
                        ? match
                        : null;

                    return new MatchComparisonLineDto
                    {
                        InvoiceLineId = l.Id,
                        LineNumber = l.LineNumber,
                        Description = l.Description,
                        QuantityOrdered = poLine?.QuantityOrdered ?? 0m,
                        PoUnitPrice = poLine?.UnitPrice.Amount ?? 0m,
                        QuantityReceived = poLine?.QuantityReceived ?? 0m,
                        QuantityInvoiced = l.Quantity,
                        InvoiceUnitPrice = l.UnitPrice.Amount
                    };
                })
                .ToList()
        };
    }
}
