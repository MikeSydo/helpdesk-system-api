using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Tests.Integration;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(
    options,
    logger,
    encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        ClaimsIdentity identity = new(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.Parse(
                "11111111-1111-1111-1111-111111111111").ToString()),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Email, "test-admin@example.com")
        ],
        "Test");

        return Task.FromResult(
            AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
