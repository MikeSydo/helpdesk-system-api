using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using HelpDesk.Tests.Integration;

namespace HelpDesk.Tests.DataAccess;

public class UserRepositoryTests(CustomWebApplicationFactory factory) :
    IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task AddAndGetById_ShouldPersistUser()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User user = User.Create(
            "Repository User",
            $"repository-{Guid.NewGuid():N}@example.com",
            "hash");

        await repository.AddAsync(user);

        User? result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.True(result.IsActive);
        Assert.InRange(
            result.UpdatedAt,
            user.UpdatedAt.AddMilliseconds(-1),
            user.UpdatedAt.AddMilliseconds(1));
    }

    [Fact]
    public async Task GetUserByEmail_ShouldTrimEmailAndReturnUser()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User user = User.Create(
            "Email User",
            $"email-{Guid.NewGuid():N}@example.com",
            "hash");
        await repository.AddAsync(user);

        User? result = await repository.GetUserByEmailAsync($"  {user.Email}  ");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnUsersInCreatedAtThenNameOrder()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User first = User.Create("A User", $"all-a-{Guid.NewGuid():N}@example.com", "hash");
        User second = User.Create("B User", $"all-b-{Guid.NewGuid():N}@example.com", "hash");

        await repository.AddAsync(first);
        await repository.AddAsync(second);

        IReadOnlyList<User> result = await repository.GetAllUsers();

        int firstIndex = result.ToList().FindIndex(user => user.Id == first.Id);
        int secondIndex = result.ToList().FindIndex(user => user.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(firstIndex < secondIndex);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistRoleActiveStateAndUpdatedAt()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User user = User.Create(
            "Update User",
            $"update-{Guid.NewGuid():N}@example.com",
            "hash");
        await repository.AddAsync(user);

        DateTime originalUpdatedAt = user.UpdatedAt;
        user.ChangeRole(UserRole.Support);
        user.Deactivate();

        await repository.UpdateAsync(user);

        User? result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(UserRole.Support, result.Role);
        Assert.False(result.IsActive);
        Assert.True(result.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ShouldReturnNull()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User? result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
