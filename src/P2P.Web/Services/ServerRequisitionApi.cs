using P2P.Application.Abstractions.Messaging;
using P2P.Application.Requisitions.Commands.ApproveRequisition;
using P2P.Application.Requisitions.Commands.CancelRequisition;
using P2P.Application.Requisitions.Commands.CreateRequisition;
using P2P.Application.Requisitions.Commands.RejectRequisition;
using P2P.Application.Requisitions.Commands.SubmitRequisition;
using P2P.Application.Requisitions.Queries.GetApprovalInbox;
using P2P.Application.Requisitions.Queries.GetRequisitionById;
using P2P.Application.Requisitions.Queries.GetRequisitionList;
using P2P.Contracts.Common;
using P2P.Contracts.Requisitions;
using P2P.Web.Endpoints;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IRequisitionApi"/>.</summary>
internal sealed class ServerRequisitionApi(IDispatcher dispatcher) : IRequisitionApi
{
    public async Task<PagedResult<RequisitionListItemDto>> GetListAsync(
        RequisitionGridQuery query, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetRequisitionListQuery(query), ct);

        return result.IsSuccess
            ? result.Value
            : PagedResult<RequisitionListItemDto>.Empty();
    }

    public async Task<PagedResult<RequisitionListItemDto>> GetApprovalInboxAsync(
        GridQuery query, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetApprovalInboxQuery(query), ct);

        return result.IsSuccess
            ? result.Value
            : PagedResult<RequisitionListItemDto>.Empty();
    }

    public async Task<RequisitionDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetRequisitionByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<OperationResult<Guid>> CreateAsync(
        CreateRequisitionRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new CreateRequisitionCommand(request), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new SubmitRequisitionCommand(id), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> ApproveAsync(
        Guid id, string? comment, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new ApproveRequisitionCommand(id, comment), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> RejectAsync(
        Guid id, string reason, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new RejectRequisitionCommand(id, reason), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new CancelRequisitionCommand(id), ct);
        return result.ToOperationResult();
    }
}
