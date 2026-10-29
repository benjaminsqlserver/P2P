using P2P.Contracts.Common;

namespace P2P.Contracts.Invoices;

public interface IInvoiceApi
{
    Task<PagedResult<InvoiceExceptionRowDto>> GetExceptionQueueAsync(
        InvoiceExceptionQuery query, CancellationToken ct = default);

    Task<InvoiceDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<OperationResult> RunMatchAsync(Guid id, CancellationToken ct = default);

    Task<OperationResult> ResolveExceptionAsync(
        Guid invoiceId, Guid exceptionId, string resolution,
        CancellationToken ct = default);
}
