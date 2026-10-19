using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDesk.Application.Users;
using HelpDesk.Application.Users.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.Integration;

public class UsersApiTests(CustomWebApplicationFactory factory) :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        HttpResponseMessage response =
            await _client.GetAsync($"/api/Users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenUserExists_ShouldReturnUserWithoutPasswordHash()
    {
        User user = await SeedUserAsync();

        HttpResponseMessage response =
            await _client.GetAsync($"/api/Users/{user.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        JsonDocument document = JsonDocument.Parse(body);

        Assert.False(document.RootElement.TryGetProperty("passwordHash", out _));
        Assert.True(document.RootElement.TryGetProperty("updatedAt", out _));
        Assert.True(document.RootElement.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangeRole_WhenUserExists_ShouldReturnUpdatedUser()
    {
        User user = await SeedUserAsync();

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/Users/{user.Id}/role",
            new ChangeRoleRequest(UserRole.Support));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserResponse? body = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(body);
        Assert.Equal("Support", body.Role);
        Assert.Equal(user.Id, body.Id);
    }

    [Fact]
    public async Task Deactivate_WhenUserExists_ShouldReturnNoContent()
    {
        User user = await SeedUserAsync();

        HttpResponseMessage response =
            await _client.DeleteAsync($"/api/Users/{user.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ChangeRole_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/Users/{Guid.NewGuid()}/role",
            new ChangeRoleRequest(UserRole.Support));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<User> SeedUserAsync()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User user = User.Create(
            "Integration User",
            $"integration-{Guid.NewGuid():N}@example.com",
            "integration-secret-hash");

        await repository.AddAsync(user);
        return user;
    }
}
