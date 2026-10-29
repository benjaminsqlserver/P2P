using P2P.Application.Abstractions.Messaging;
using P2P.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using P2P.Application.GoodsReceipts.Queries.GetGoodsReceiptById;
using P2P.Contracts.Common;
using P2P.Contracts.GoodsReceipts;

namespace P2P.Web.Endpoints;

public sealed class GoodsReceiptEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goods-receipts")
            .WithTags("GoodsReceipts")
            .RequireAuthorization(Policies.CanRecordReceipts);

        group.MapPost("/",
            async (CreateGoodsReceiptRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreateGoodsReceiptCommand(request), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .WithName("CreateGoodsReceipt");

        group.MapGet("/{id:guid}",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetGoodsReceiptByIdQuery(id), ct);
                return result.ToHttpResult();
            })
            .WithName("GetGoodsReceiptById");
    }
}
