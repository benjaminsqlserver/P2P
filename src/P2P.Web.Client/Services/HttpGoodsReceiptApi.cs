using System.Net.Http.Json;
using P2P.Contracts.Common;
using P2P.Contracts.GoodsReceipts;

namespace P2P.Web.Client.Services;

/// <summary>The WebAssembly implementation of <see cref="IGoodsReceiptApi"/>.</summary>
internal sealed class HttpGoodsReceiptApi(HttpClient http) : IGoodsReceiptApi
{
    public async Task<OperationResult<Guid>> CreateAsync(
        CreateGoodsReceiptRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/goods-receipts", request, ct);
        return await HttpResponses.ReadResultAsync<Guid>(response, ct);
    }

    public async Task<GoodsReceiptDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/goods-receipts/{id}", ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GoodsReceiptDetailDto>(ct)
            : null;
    }
}
