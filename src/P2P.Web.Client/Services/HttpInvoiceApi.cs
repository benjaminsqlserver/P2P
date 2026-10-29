using System.Net.Http.Json;
using P2P.Contracts.Common;
using P2P.Contracts.Invoices;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IInvoiceApi"/>.</summary>
internal sealed class HttpInvoiceApi(HttpClient http) : IInvoiceApi
{
    public async Task<PagedResult<InvoiceExceptionRowDto>> GetExceptionQueueAsync(
        InvoiceExceptionQuery query, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            "api/invoices/exceptions/search", query, ct);

        if (!response.IsSuccessStatusCode)
        {
            return PagedResult<InvoiceExceptionRowDto>.Empty();
        }

        return await response.Content
            .ReadFromJsonAsync<PagedResult<InvoiceExceptionRowDto>>(ct)
            ?? PagedResult<InvoiceExceptionRowDto>.Empty();
    }

    public async Task<InvoiceDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/invoices/{id}", ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<InvoiceDetailDto>(ct)
            : null;
    }

    public async Task<OperationResult> RunMatchAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/invoices/{id}/match", null, ct);
        return await HttpResponses.ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> ResolveExceptionAsync(
        Guid invoiceId, Guid exceptionId, string resolution, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/invoices/{invoiceId}/exceptions/{exceptionId}/resolve",
            new ReasonRequest(resolution), ct);

        return await HttpResponses.ReadResultAsync(response, ct);
    }
}
