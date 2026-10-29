using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.GoodsReceipts;
using P2P.Domain.Common;
using P2P.Domain.GoodsReceipts;

namespace P2P.Application.GoodsReceipts.Queries.GetGoodsReceiptById;

public sealed record GetGoodsReceiptByIdQuery(Guid GoodsReceiptId)
    : IQuery<GoodsReceiptDetailDto>;

internal sealed class GetGoodsReceiptByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetGoodsReceiptByIdQuery, GoodsReceiptDetailDto>
{
    public async Task<Result<GoodsReceiptDetailDto>> Handle(
        GetGoodsReceiptByIdQuery request, CancellationToken ct)
    {
        var receipt = await context.GoodsReceipts
            .AsNoTracking()
            .Include(g => g.Lines)
            .SingleOrDefaultAsync(g => g.Id == request.GoodsReceiptId, ct);

        if (receipt is null)
        {
            return Result.Failure<GoodsReceiptDetailDto>(
                Error.NotFound(nameof(GoodsReceipt), request.GoodsReceiptId));
        }

        var order = await context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Lines)
            .SingleOrDefaultAsync(p => p.Id == receipt.PurchaseOrderId, ct);

        var vendorName = order is null
            ? "—"
            : await context.Vendors
                .AsNoTracking()
                .Where(v => v.Id == order.VendorId)
                .Select(v => v.LegalName)
                .FirstOrDefaultAsync(ct) ?? "—";

        var descriptions = order?.Lines.ToDictionary(l => l.Id, l => l.Description) ?? [];

        return new GoodsReceiptDetailDto
        {
            Id = receipt.Id,
            Number = receipt.Number,
            PurchaseOrderId = receipt.PurchaseOrderId,
            PurchaseOrderNumber = order?.Number ?? "—",
            VendorName = vendorName,
            ReceivedOn = receipt.ReceivedOn,
            DeliveryNoteReference = receipt.DeliveryNoteReference,
            Carrier = receipt.Carrier,
            IsReversed = receipt.IsReversed,
            ReversalReason = receipt.ReversalReason,
            Lines = receipt.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new GoodsReceiptLineDto
                {
                    LineNumber = l.LineNumber,
                    PurchaseOrderLineId = l.PurchaseOrderLineId,
                    Description = descriptions.GetValueOrDefault(l.PurchaseOrderLineId, "—"),
                    QuantityAccepted = l.QuantityAccepted,
                    QuantityRejected = l.QuantityRejected,
                    RejectionReason = l.RejectionReason
                })
                .ToList()
        };
    }
}
