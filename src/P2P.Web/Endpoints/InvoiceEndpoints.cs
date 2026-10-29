using P2P.Application.Abstractions.Messaging;
using P2P.Application.Invoices.Commands.ResolveMatchException;
using P2P.Application.Invoices.Commands.RunThreeWayMatch;
using P2P.Application.Invoices.Queries.GetInvoiceById;
using P2P.Application.Invoices.Queries.GetInvoiceExceptionQueue;
using P2P.Contracts.Common;
using P2P.Contracts.Invoices;

namespace P2P.Web.Endpoints;

public sealed class InvoiceEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .RequireAuthorization(Policies.CanProcessInvoices);

        group.MapPost("/exceptions/search",
            async (InvoiceExceptionQuery query, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(
                    new GetInvoiceExceptionQueueQuery(query), ct);
                return result.ToHttpResult();
            })
            .WithName("GetInvoiceExceptionQueue");

        group.MapGet("/{id:guid}",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetInvoiceByIdQuery(id), ct);
                return result.ToHttpResult();
            })
            .WithName("GetInvoiceById");

        group.MapPost("/{id:guid}/match",
            async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RunThreeWayMatchCommand(id), ct);
                return Results.Ok(result.ToOperationResult());
            })
            .WithName("RunThreeWayMatch");

        group.MapPost("/{invoiceId:guid}/exceptions/{exceptionId:guid}/resolve",
            async (Guid invoiceId,
                   Guid exceptionId,
                   ReasonRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new ResolveMatchExceptionCommand(invoiceId, exceptionId, request.Reason),
                    ct);

                return Results.Ok(result.ToOperationResult());
            })
            .WithName("ResolveMatchException");
    }
}
