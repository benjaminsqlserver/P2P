using P2P.Application.Abstractions.Messaging;
using P2P.Application.Payments.Commands.CreatePaymentRun;
using P2P.Application.Payments.Queries.GetPayableInvoices;
using P2P.Contracts.Common;
using P2P.Contracts.Payments;
using P2P.Web.Endpoints;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IPaymentApi"/>.</summary>
internal sealed class ServerPaymentApi(IDispatcher dispatcher) : IPaymentApi
{
    public async Task<IReadOnlyList<PayableInvoiceDto>> GetPayablesAsync(
        DateOnly dueOnOrBefore, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetPayableInvoicesQuery(dueOnOrBefore), ct);
        return result.IsSuccess ? result.Value : [];
    }

    public async Task<OperationResult<IReadOnlyList<Guid>>> CreatePaymentRunAsync(
        CreatePaymentRunRequest request, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new CreatePaymentRunCommand(request), ct);
        return result.ToOperationResult();
    }
}
