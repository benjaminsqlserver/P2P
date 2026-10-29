using System.Net.Http.Json;
using P2P.Contracts.Common;
using P2P.Contracts.Vendors;

namespace P2P.Web.Client.Services;

/// <summary>
/// The WebAssembly implementation of <see cref="IVendorApi"/>. Calls the
/// minimal API endpoints defined in <c>VendorEndpoints</c>.
/// </summary>
internal sealed class HttpVendorApi(HttpClient http) : IVendorApi
{
    public async Task<PagedResult<VendorListItemDto>> GetListAsync(
        VendorGridQuery query, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/vendors/search", query, ct);

        if (!response.IsSuccessStatusCode)
        {
            return PagedResult<VendorListItemDto>.Empty();
        }

        return await response.Content
            .ReadFromJsonAsync<PagedResult<VendorListItemDto>>(ct)
            ?? PagedResult<VendorListItemDto>.Empty();
    }

    public async Task<VendorDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/vendors/{id}", ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<VendorDetailDto>(ct)
            : null;
    }

    public async Task<IReadOnlyList<VendorLookupDto>> SearchLookupAsync(
        string? term, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(term)
            ? "api/vendors/lookup"
            : $"api/vendors/lookup?term={Uri.EscapeDataString(term)}";

        return await http.GetFromJsonAsync<List<VendorLookupDto>>(url, ct) ?? [];
    }

    public async Task<OperationResult<Guid>> CreateAsync(
        CreateVendorRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/vendors", request, ct);
        return await ReadResultAsync<Guid>(response, ct);
    }

    public async Task<OperationResult> UpdateAsync(
        UpdateVendorRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync(
            $"api/vendors/{request.Id}", request, ct);

        return await ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> ActivateAsync(
        Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/vendors/{id}/activate", null, ct);
        return await ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> PlaceOnHoldAsync(
        Guid id, string reason, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/vendors/{id}/hold", new { Reason = reason }, ct);

        return await ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> AddBankAccountAsync(
        AddBankAccountRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/vendors/{request.VendorId}/bank-accounts", request, ct);

        return await ReadResultAsync(response, ct);
    }

    private static async Task<OperationResult> ReadResultAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<OperationResult>(ct)
                ?? OperationResult.Success();
        }

        return OperationResult.Failure(
            "Http.Error",
            $"The server returned {(int)response.StatusCode} "
          + $"{response.ReasonPhrase}.");
    }

    private static async Task<OperationResult<T>> ReadResultAsync<T>(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<OperationResult<T>>(ct)
                ?? OperationResult<T>.Failure(
                    "Http.EmptyResponse", "The server returned no content.");
        }

        return OperationResult<T>.Failure(
            "Http.Error",
            $"The server returned {(int)response.StatusCode} "
          + $"{response.ReasonPhrase}.");
    }
}
