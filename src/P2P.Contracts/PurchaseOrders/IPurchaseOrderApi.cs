using P2P.Contracts.Common;

namespace P2P.Contracts.PurchaseOrders;

public interface IPurchaseOrderApi
{
    Task<IReadOnlyList<PurchaseOrderLookupDto>> SearchOpenAsync(
        string? term, CancellationToken ct = default);

    Task<PurchaseOrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<OperationResult<Guid>> CreateFromRequisitionAsync(
        CreatePurchaseOrderFromRequisitionRequest request, CancellationToken ct = default);

    Task<OperationResult> IssueAsync(Guid id, CancellationToken ct = default);
}
