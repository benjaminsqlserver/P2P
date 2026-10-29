using System.Net.Http.Json;
using P2P.Contracts.Common;
using P2P.Contracts.Requisitions;

namespace P2P.Web.Client.Services;

/// <summary>
/// The WebAssembly implementation of <see cref="IRequisitionApi"/>. Calls the
/// minimal API endpoints defined in <c>RequisitionEndpoints</c>.
/// </summary>
internal sealed class HttpRequisitionApi(HttpClient http) : IRequisitionApi
{
    public async Task<PagedResult<RequisitionListItemDto>> GetListAsync(
        RequisitionGridQuery query, CancellationToken ct = default) =>
        await ReadPageAsync(
            await http.PostAsJsonAsync("api/requisitions/search", query, ct), ct);

    public async Task<PagedResult<RequisitionListItemDto>> GetApprovalInboxAsync(
        GridQuery query, CancellationToken ct = default) =>
        await ReadPageAsync(
            await http.PostAsJsonAsync("api/requisitions/inbox", query, ct), ct);

    public async Task<RequisitionDetailDto?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/requisitions/{id}", ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RequisitionDetailDto>(ct)
            : null;
    }

    public async Task<OperationResult<Guid>> CreateAsync(
        CreateRequisitionRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/requisitions", request, ct);
        return await HttpResponses.ReadResultAsync<Guid>(response, ct);
    }

    public async Task<OperationResult> SubmitAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/requisitions/{id}/submit", null, ct);
        return await HttpResponses.ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> ApproveAsync(
        Guid id, string? comment, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/requisitions/{id}/approve", new CommentRequest(comment), ct);

        return await HttpResponses.ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> RejectAsync(
        Guid id, string reason, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/requisitions/{id}/reject", new ReasonRequest(reason), ct);

        return await HttpResponses.ReadResultAsync(response, ct);
    }

    public async Task<OperationResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/requisitions/{id}/cancel", null, ct);
        return await HttpResponses.ReadResultAsync(response, ct);
    }

    private static async Task<PagedResult<RequisitionListItemDto>> ReadPageAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            return PagedResult<RequisitionListItemDto>.Empty();
        }

        return await response.Content
            .ReadFromJsonAsync<PagedResult<RequisitionListItemDto>>(ct)
            ?? PagedResult<RequisitionListItemDto>.Empty();
    }
}
