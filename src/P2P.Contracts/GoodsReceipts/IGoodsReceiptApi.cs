using P2P.Contracts.Common;

namespace P2P.Contracts.GoodsReceipts;

public interface IGoodsReceiptApi
{
    Task<OperationResult<Guid>> CreateAsync(
        CreateGoodsReceiptRequest request, CancellationToken ct = default);

    Task<GoodsReceiptDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
