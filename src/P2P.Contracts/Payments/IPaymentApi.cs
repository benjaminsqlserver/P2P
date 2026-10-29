using P2P.Contracts.Common;

namespace P2P.Contracts.Payments;

public interface IPaymentApi
{
    Task<IReadOnlyList<PayableInvoiceDto>> GetPayablesAsync(
        DateOnly dueOnOrBefore, CancellationToken ct = default);

    /// <summary>
    /// Creates one draft payment per vendor and currency for the selected
    /// invoices. Returns the ids of the payments created.
    /// </summary>
    Task<OperationResult<IReadOnlyList<Guid>>> CreatePaymentRunAsync(
        CreatePaymentRunRequest request, CancellationToken ct = default);
}
