using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Week2Api.Auth;

/// <summary>
/// Minimal token authentication for the exercise. It maps a bearer token to a
/// user + role so the pipeline can produce real 401 (no/invalid token) and
/// 403 (authenticated but wrong role) responses without JWT infrastructure.
///
///   Authorization: Bearer admin-token  -> role "Admin"
///   Authorization: Bearer user-token    -> role "User"
/// </summary>
public class TokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Token";

    private static readonly Dictionary<string, (string User, string Role)> Tokens = new()
    {
        ["admin-token"] = ("admin", "Admin"),
        ["user-token"] = ("reader", "User"),
    };

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return Task.FromResult(AuthenticateResult.NoResult());

        var value = header.ToString();
        if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header."));

        var token = value["Bearer ".Length..].Trim();
        if (!Tokens.TryGetValue(token, out var principalInfo))
            return Task.FromResult(AuthenticateResult.Fail("Invalid token."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, principalInfo.User),
            new Claim(ClaimTypes.Role, principalInfo.Role),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
