using P2P.Application.Abstractions.Messaging;
using P2P.Application.Invoices.Commands.ResolveMatchException;
using P2P.Application.Invoices.Commands.RunThreeWayMatch;
using P2P.Application.Invoices.Queries.GetInvoiceById;
using P2P.Application.Invoices.Queries.GetInvoiceExceptionQueue;
using P2P.Contracts.Common;
using P2P.Contracts.Invoices;
using P2P.Web.Endpoints;

namespace P2P.Web.Services;

/// <summary>The in-process implementation of <see cref="IInvoiceApi"/>.</summary>
internal sealed class ServerInvoiceApi(IDispatcher dispatcher) : IInvoiceApi
{
    public async Task<PagedResult<InvoiceExceptionRowDto>> GetExceptionQueueAsync(
        InvoiceExceptionQuery query, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetInvoiceExceptionQueueQuery(query), ct);

        return result.IsSuccess
            ? result.Value
            : PagedResult<InvoiceExceptionRowDto>.Empty();
    }

    public async Task<InvoiceDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Query(new GetInvoiceByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<OperationResult> RunMatchAsync(Guid id, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(new RunThreeWayMatchCommand(id), ct);
        return result.ToOperationResult();
    }

    public async Task<OperationResult> ResolveExceptionAsync(
        Guid invoiceId, Guid exceptionId, string resolution, CancellationToken ct = default)
    {
        var result = await dispatcher.Send(
            new ResolveMatchExceptionCommand(invoiceId, exceptionId, resolution), ct);

        return result.ToOperationResult();
    }
}
