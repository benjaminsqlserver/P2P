using System.Net.Http.Json;
using P2P.Contracts.Common;
using P2P.Contracts.PurchaseOrders;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IPurchaseOrderApi"/>.</summary>
internal sealed class HttpPurchaseOrderApi(HttpClient http) : IPurchaseOrderApi
{
    public async Task<IReadOnlyList<PurchaseOrderLookupDto>> SearchOpenAsync(
        string? term, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(term)
            ? "api/purchase-orders/open"
            : $"api/purchase-orders/open?term={Uri.EscapeDataString(term)}";

        return await http.GetFromJsonAsync<List<PurchaseOrderLookupDto>>(url, ct) ?? [];
    }

    public async Task<PurchaseOrderDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/purchase-orders/{id}", ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PurchaseOrderDetailDto>(ct)
            : null;
    }

    public async Task<OperationResult<Guid>> CreateFromRequisitionAsync(
        CreatePurchaseOrderFromRequisitionRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/purchase-orders", request, ct);
        return await HttpResponses.ReadResultAsync<Guid>(response, ct);
    }

    public async Task<OperationResult> IssueAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/purchase-orders/{id}/issue", null, ct);
        return await HttpResponses.ReadResultAsync(response, ct);
    }
}
