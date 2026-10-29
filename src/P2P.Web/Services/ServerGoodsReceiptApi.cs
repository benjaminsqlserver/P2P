using P2P.Application.Abstractions.Messaging;
using P2P.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using P2P.Application.GoodsReceipts.Queries.GetGoodsReceiptById;
using P2P.Contracts.Common;
using P2P.Contracts.GoodsReceipts;
using P2P.Web.Endpoints;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IGoodsReceiptApi"/>.</summary>
internal sealed class ServerGoodsReceiptApi(IDispatcher dispatcher) : IGoodsReceiptApi
{
    public async Task<OperationResult<Guid>> CreateAsync(
        CreateGoodsReceiptRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new CreateGoodsReceiptCommand(request), ct);
        return result.ToOperationResult();
    }

    public async Task<GoodsReceiptDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetGoodsReceiptByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }
}
