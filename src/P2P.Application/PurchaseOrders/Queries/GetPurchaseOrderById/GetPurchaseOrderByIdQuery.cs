using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Contracts.PurchaseOrders;
using P2P.Contracts.Vendors;
using P2P.Domain.Common;
using P2P.Domain.PurchaseOrders;

namespace P2P.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdQuery(Guid PurchaseOrderId)
    : IQuery<PurchaseOrderDetailDto>;

internal sealed class GetPurchaseOrderByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto>
{
    public async Task<Result<PurchaseOrderDetailDto>> Handle(
        GetPurchaseOrderByIdQuery request, CancellationToken ct)
    {
        var order = await context.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Lines)
            .SingleOrDefaultAsync(p => p.Id == request.PurchaseOrderId, ct);

        if (order is null)
        {
            return Result.Failure<PurchaseOrderDetailDto>(
                Error.NotFound(nameof(PurchaseOrder), request.PurchaseOrderId));
        }

        var vendorName = await context.Vendors
            .AsNoTracking()
            .Where(v => v.Id == order.VendorId)
            .Select(v => v.LegalName)
            .FirstOrDefaultAsync(ct) ?? "—";

        return new PurchaseOrderDetailDto
        {
            Id = order.Id,
            Number = order.Number,
            VendorId = order.VendorId,
            VendorName = vendorName,
            RequisitionId = order.RequisitionId,
            Status = (PurchaseOrderStatusDto)(int)order.Status,
            Currency = order.Currency,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            PaymentTermsDays = order.PaymentTermsDays,
            ShipToAddress = new AddressDto
            {
                Line1 = order.ShipToAddress.Line1,
                Line2 = order.ShipToAddress.Line2,
                City = order.ShipToAddress.City,
                StateOrProvince = order.ShipToAddress.StateOrProvince,
                PostalCode = order.ShipToAddress.PostalCode,
                CountryCode = order.ShipToAddress.CountryCode
            },
            NetTotal = order.NetTotal.Amount,
            TaxTotal = order.TaxTotal.Amount,
            GrossTotal = order.GrossTotal.Amount,
            Lines = order.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new PurchaseOrderLineDto
                {
                    Id = l.Id,
                    LineNumber = l.LineNumber,
                    ItemCode = l.ItemCode,
                    Description = l.Description,
                    QuantityOrdered = l.QuantityOrdered,
                    UnitOfMeasure = l.UnitOfMeasure,
                    UnitPrice = l.UnitPrice.Amount,
                    TaxRatePercent = l.TaxRatePercent,
                    QuantityReceived = l.QuantityReceived,
                    QuantityInvoiced = l.QuantityInvoiced,
                    NetTotal = l.NetTotal.Amount
                })
                .ToList()
        };
    }
}
