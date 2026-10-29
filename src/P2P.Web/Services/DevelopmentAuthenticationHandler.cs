using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace P2P.Web.Services;

/// <summary>The fixed identity used by <see cref="DevelopmentAuthenticationHandler"/>.</summary>
public sealed class DevelopmentUserOptions
{
    public const string SectionName = "DevelopmentUser";

    public Guid UserId { get; set; }

    public string Name { get; set; } = "Developer";

    public IReadOnlyList<string> Roles { get; set; } = [];
}

/// <summary>
/// Signs every request in as the user configured under
/// <c>DevelopmentUser</c>, so the application can be run before a real
/// identity provider is wired up (chapter 19). Registered only in the
/// Development environment; it must never be enabled anywhere else.
/// </summary>
internal sealed class DevelopmentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<DevelopmentUserOptions> developmentUser)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Development";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = developmentUser.Value;

        if (user.UserId == Guid.Empty)
        {
            return Task.FromResult(AuthenticateResult.Fail(
                $"{DevelopmentUserOptions.SectionName}:UserId is not configured."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Name)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
