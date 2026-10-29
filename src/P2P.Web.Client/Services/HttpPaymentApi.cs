using System.Globalization;
using System.Net.Http.Json;
using P2P.Contracts.Common;
using P2P.Contracts.Payments;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IPaymentApi"/>.</summary>
internal sealed class HttpPaymentApi(HttpClient http) : IPaymentApi
{
    public async Task<IReadOnlyList<PayableInvoiceDto>> GetPayablesAsync(
        DateOnly dueOnOrBefore, CancellationToken ct = default)
    {
        var date = dueOnOrBefore.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return await http.GetFromJsonAsync<List<PayableInvoiceDto>>(
            $"api/payments/payables?dueOnOrBefore={date}", ct) ?? [];
    }

    public async Task<OperationResult<IReadOnlyList<Guid>>> CreatePaymentRunAsync(
        CreatePaymentRunRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/payments/runs", request, ct);
        return await HttpResponses.ReadResultAsync<IReadOnlyList<Guid>>(response, ct);
    }
}
