using P2P.Application.Abstractions.Messaging;
using P2P.Application.Payments.Commands.CreatePaymentRun;
using P2P.Application.Payments.Queries.GetPayableInvoices;
using P2P.Contracts.Common;
using P2P.Contracts.Payments;

namespace P2P.Web.Endpoints;

public sealed class PaymentEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .RequireAuthorization(Policies.CanAuthorisePayments);

        group.MapGet("/payables",
            async (DateOnly dueOnOrBefore, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(
                    new GetPayableInvoicesQuery(dueOnOrBefore), ct);
                return result.ToHttpResult();
            })
            .WithName("GetPayableInvoices");

        group.MapPost("/runs",
            async (CreatePaymentRunRequest request,
                   IDispatcher dispatcher,
                   CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreatePaymentRunCommand(request), ct);

                return Results.Ok(result.ToOperationResult());
            })
            .WithName("CreatePaymentRun");
    }
}
