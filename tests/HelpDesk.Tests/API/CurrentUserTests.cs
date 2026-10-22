using System.Security.Claims;
using HelpDesk.API.Security;
using HelpDesk.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HelpDesk.Tests.API;

public class CurrentUserTests
{
    [Fact]
    public void CurrentUser_WithValidClaims_ShouldExposeIdentityAndRole()
    {
        Guid userId = Guid.NewGuid();
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, nameof(UserRole.Support))
        ], "Test"));

        CurrentUser currentUser = new(new HttpContextAccessor
        {
            HttpContext = httpContext
        });

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.UserId);
        Assert.Equal(UserRole.Support, currentUser.Role);
    }

    [Fact]
    public void CurrentUser_WithMalformedClaims_ShouldRemainSafe()
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid"),
            new Claim(ClaimTypes.Role, "Unknown")
        ], "Test"));

        CurrentUser currentUser = new(new HttpContextAccessor
        {
            HttpContext = httpContext
        });

        Assert.True(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
        Assert.Null(currentUser.Role);
    }
}
