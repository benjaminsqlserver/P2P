using P2P.Application.Abstractions.Messaging;
using P2P.Application.PurchaseOrders.Commands.CreatePurchaseOrderFromRequisition;
using P2P.Application.PurchaseOrders.Commands.IssuePurchaseOrder;
using P2P.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using P2P.Application.PurchaseOrders.Queries.SearchOpenPurchaseOrders;
using P2P.Contracts.Common;
using P2P.Contracts.PurchaseOrders;

namespace P2P.Web.Endpoints;

public sealed class PurchaseOrderEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchase-orders")
            .WithTags("PurchaseOrders")
            .RequireAuthorization();

        group.MapGet("/open",
            async (string? term, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(
                    new SearchOpenPurchaseOrdersQuery(term), ct);
                return result.ToHttpResult();
            })
            .WithName("SearchOpenPurchaseOrders");

        group.MapGet("/{id:guid}",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetPurchaseOrderByIdQuery(id), ct);
                return result.ToHttpResult();
            })
            .WithName("GetPurchaseOrderById");

        group.MapPost("/",
            async (CreatePurchaseOrderFromRequisitionRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreatePurchaseOrderFromRequisitionCommand(request), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(Policies.CanIssuePurchaseOrders)
            .WithName("CreatePurchaseOrderFromRequisition");

        group.MapPost("/{id:guid}/issue",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new IssuePurchaseOrderCommand(id), ct);
                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(Policies.CanIssuePurchaseOrders)
            .WithName("IssuePurchaseOrder");
    }
}
