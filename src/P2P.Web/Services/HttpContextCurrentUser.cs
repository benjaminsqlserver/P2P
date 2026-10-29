using System.Security.Claims;
using P2P.Application.Abstractions;

namespace P2P.Web.Services;

internal sealed class HttpContextCurrentUser(IHttpContextAccessor accessor)
    : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(
            Principal?.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id)
            ? id
            : Guid.Empty;

    public string UserName =>
        Principal?.FindFirstValue(ClaimTypes.Name) ?? "system";

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
