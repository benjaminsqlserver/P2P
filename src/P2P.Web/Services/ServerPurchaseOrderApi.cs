using P2P.Application.Abstractions.Messaging;
using P2P.Application.PurchaseOrders.Commands.CreatePurchaseOrderFromRequisition;
using P2P.Application.PurchaseOrders.Commands.IssuePurchaseOrder;
using P2P.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using P2P.Application.PurchaseOrders.Queries.SearchOpenPurchaseOrders;
using P2P.Contracts.Common;
using P2P.Contracts.PurchaseOrders;
using P2P.Web.Endpoints;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IPurchaseOrderApi"/>.</summary>
internal sealed class ServerPurchaseOrderApi(IDispatcher dispatcher) : IPurchaseOrderApi
{
    public async Task<IReadOnlyList<PurchaseOrderLookupDto>> SearchOpenAsync(
        string? term, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new SearchOpenPurchaseOrdersQuery(term), ct);
        return result.IsSuccess ? result.Value : [];
    }

    public async Task<PurchaseOrderDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetPurchaseOrderByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<OperationResult<Guid>> CreateFromRequisitionAsync(
        CreatePurchaseOrderFromRequisitionRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(
            new CreatePurchaseOrderFromRequisitionCommand(request), ct);

        return result.ToOperationResult();
    }

    public async Task<OperationResult> IssueAsync(Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new IssuePurchaseOrderCommand(id), ct);
        return result.ToOperationResult();
    }
}
