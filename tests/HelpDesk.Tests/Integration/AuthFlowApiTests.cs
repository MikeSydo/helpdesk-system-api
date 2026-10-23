using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDesk.Application.Auth.Dtos;

namespace HelpDesk.Tests.Integration;

public sealed class AuthFlowApiTests(JwtAuthWebApplicationFactory factory) :
    IClassFixture<JwtAuthWebApplicationFactory>
{
    [Fact]
    public async Task FullAuthFlow_ShouldRotateLogoutAndDeleteAccount()
    {
        using HttpClient client = factory.CreateClient();
        string email = $"auth-flow-{Guid.NewGuid():N}@example.com";
        RegisterRequest registration = new(
            "Auth Flow User",
            email,
            "Password1",
            "Password1");

        HttpResponseMessage registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            registration);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        await ReadAuthResponse(registerResponse);

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "Password1"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        AuthResponse loggedIn = await ReadAuthResponse(loginResponse);

        HttpResponseMessage refreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(loggedIn.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        AuthResponse refreshed = await ReadAuthResponse(refreshResponse);
        Assert.NotEqual(loggedIn.RefreshToken, refreshed.RefreshToken);

        HttpResponseMessage reusedRefreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(loggedIn.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reusedRefreshResponse.StatusCode);

        HttpResponseMessage loginAgainResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "Password1"));
        Assert.Equal(HttpStatusCode.OK, loginAgainResponse.StatusCode);
        AuthResponse beforeLogout = await ReadAuthResponse(loginAgainResponse);

        HttpResponseMessage logoutResponse = await client.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutRequest(beforeLogout.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        HttpResponseMessage loggedOutRefreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(beforeLogout.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, loggedOutRefreshResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            beforeLogout.AccessToken);

        HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/auth/account");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage inactiveLoginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "Password1"));
        Assert.Equal(HttpStatusCode.Unauthorized, inactiveLoginResponse.StatusCode);

        HttpResponseMessage deletedRefreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(refreshed.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, deletedRefreshResponse.StatusCode);
    }

    private static async Task<AuthResponse> ReadAuthResponse(HttpResponseMessage response)
    {
        AuthResponse? result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        return result;
    }
}
