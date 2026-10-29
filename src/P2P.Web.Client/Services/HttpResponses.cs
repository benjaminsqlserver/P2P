using System.Net.Http.Json;
using P2P.Contracts.Common;

namespace P2P.Web.Client.Services;

/// <summary>
/// Turns an HTTP response from a command endpoint into the
/// <see cref="OperationResult"/> the UI expects, so a transport failure
/// looks like any other failure to the calling component.
/// </summary>
internal static class HttpResponses
{
    public static async Task<OperationResult> ReadResultAsync(
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

    public static async Task<OperationResult<T>> ReadResultAsync<T>(
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
