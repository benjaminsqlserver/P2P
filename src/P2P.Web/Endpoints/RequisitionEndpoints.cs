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

namespace P2P.Web.Endpoints;

public sealed class RequisitionEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requisitions")
            .WithTags("Requisitions")
            .RequireAuthorization();

        group.MapPost("/search",
            async (RequisitionGridQuery query, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetRequisitionListQuery(query), ct);
                return result.ToHttpResult();
            })
            .WithName("SearchRequisitions");

        group.MapPost("/inbox",
            async (GridQuery query, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetApprovalInboxQuery(query), ct);
                return result.ToHttpResult();
            })
            .RequireAuthorization(Policies.CanApproveRequisitions)
            .WithName("GetApprovalInbox");

        group.MapGet("/{id:guid}",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetRequisitionByIdQuery(id), ct);
                return result.ToHttpResult();
            })
            .WithName("GetRequisitionById");

        group.MapPost("/",
            async (CreateRequisitionRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreateRequisitionCommand(request), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .WithName("CreateRequisition");

        group.MapPost("/{id:guid}/submit",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new SubmitRequisitionCommand(id), ct);
                return Results.Ok(result.ToOperationResult());
            })
            .WithName("SubmitRequisition");

        group.MapPost("/{id:guid}/approve",
            async (Guid id,
                   CommentRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new ApproveRequisitionCommand(id, request.Comment), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(Policies.CanApproveRequisitions)
            .WithName("ApproveRequisition");

        group.MapPost("/{id:guid}/reject",
            async (Guid id,
                   ReasonRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new RejectRequisitionCommand(id, request.Reason), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .RequireAuthorization(Policies.CanApproveRequisitions)
            .WithName("RejectRequisition");

        group.MapPost("/{id:guid}/cancel",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new CancelRequisitionCommand(id), ct);
                return Results.Ok(result.ToOperationResult());
            })
            .WithName("CancelRequisition");
    }
}
