using P2P.Contracts.Common;

namespace P2P.Contracts.Requisitions;

public interface IRequisitionApi
{
    Task<PagedResult<RequisitionListItemDto>> GetListAsync(
        RequisitionGridQuery query, CancellationToken ct = default);

    Task<PagedResult<RequisitionListItemDto>> GetApprovalInboxAsync(
        GridQuery query, CancellationToken ct = default);

    Task<RequisitionDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<OperationResult<Guid>> CreateAsync(
        CreateRequisitionRequest request, CancellationToken ct = default);

    Task<OperationResult> SubmitAsync(Guid id, CancellationToken ct = default);

    Task<OperationResult> ApproveAsync(
        Guid id, string? comment, CancellationToken ct = default);

    Task<OperationResult> RejectAsync(
        Guid id, string reason, CancellationToken ct = default);

    Task<OperationResult> CancelAsync(Guid id, CancellationToken ct = default);
}
