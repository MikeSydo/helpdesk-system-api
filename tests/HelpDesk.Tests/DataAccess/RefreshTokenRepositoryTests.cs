using HelpDesk.Application.Auth;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.DataAccess;

public class RefreshTokenRepositoryTests(CustomWebApplicationFactory factory) :
    IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task AddAndGetByHash_ShouldPersistRefreshToken()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IRefreshTokenRepository repository =
            scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        User user = User.Create(
            "Refresh Token User",
            $"refresh-{Guid.NewGuid():N}@example.com",
            "hash");
        await userRepository.AddAsync(user);

        DateTime createdAt = DateTime.UtcNow;
        RefreshToken token = RefreshToken.Create(
            user.Id,
            $"token-hash-{Guid.NewGuid():N}",
            Guid.NewGuid(),
            createdAt.AddDays(30),
            createdAt);

        await repository.AddAsync(token);
        await repository.SaveChangesAsync();

        RefreshToken? result = await repository.GetByHashAsync(token.TokenHash);

        Assert.NotNull(result);
        Assert.Equal(token.Id, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(token.TokenFamilyId, result.TokenFamilyId);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByHash_ShouldTrimHash()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IRefreshTokenRepository repository =
            scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        User user = User.Create(
            "Trim Hash User",
            $"trim-hash-{Guid.NewGuid():N}@example.com",
            "hash");
        await userRepository.AddAsync(user);

        RefreshToken token = RefreshToken.Create(
            user.Id,
            $"trimmed-hash-{Guid.NewGuid():N}",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30));
        await repository.AddAsync(token);
        await repository.SaveChangesAsync();

        RefreshToken? result = await repository.GetByHashAsync($"  {token.TokenHash}  ");

        Assert.NotNull(result);
        Assert.Equal(token.Id, result.Id);
    }

    [Fact]
    public async Task Revoke_ShouldPersistReasonAndInactiveState()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IRefreshTokenRepository repository =
            scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        User user = User.Create(
            "Revoke User",
            $"revoke-{Guid.NewGuid():N}@example.com",
            "hash");
        await userRepository.AddAsync(user);

        RefreshToken token = RefreshToken.Create(
            user.Id,
            $"revoke-hash-{Guid.NewGuid():N}",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30));
        await repository.AddAsync(token);
        await repository.SaveChangesAsync();

        await repository.RevokeAsync(token.Id, "logout");
        await repository.SaveChangesAsync();

        RefreshToken? result = await repository.GetByHashAsync(token.TokenHash);

        Assert.NotNull(result);
        Assert.False(result.IsActive);
        Assert.NotNull(result.RevokedAt);
        Assert.Equal("logout", result.RevocationReason);
    }

    [Fact]
    public async Task RevokeFamily_ShouldRevokeAllTokensInFamily()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IRefreshTokenRepository repository =
            scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        User user = User.Create(
            "Family User",
            $"family-{Guid.NewGuid():N}@example.com",
            "hash");
        await userRepository.AddAsync(user);

        Guid familyId = Guid.NewGuid();
        RefreshToken first = RefreshToken.Create(
            user.Id,
            $"family-first-{Guid.NewGuid():N}",
            familyId,
            DateTime.UtcNow.AddDays(30));
        RefreshToken second = RefreshToken.Create(
            user.Id,
            $"family-second-{Guid.NewGuid():N}",
            familyId,
            DateTime.UtcNow.AddDays(30));
        RefreshToken otherFamily = RefreshToken.Create(
            user.Id,
            $"other-family-{Guid.NewGuid():N}",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30));

        await repository.AddAsync(first);
        await repository.AddAsync(second);
        await repository.AddAsync(otherFamily);
        await repository.SaveChangesAsync();

        await repository.RevokeFamilyAsync(familyId, "reuse_detected");
        await repository.SaveChangesAsync();

        RefreshToken? revokedFirst = await repository.GetByHashAsync(first.TokenHash);
        RefreshToken? revokedSecond = await repository.GetByHashAsync(second.TokenHash);
        RefreshToken? activeOther = await repository.GetByHashAsync(otherFamily.TokenHash);

        Assert.NotNull(revokedFirst);
        Assert.NotNull(revokedSecond);
        Assert.NotNull(activeOther);
        Assert.False(revokedFirst.IsActive);
        Assert.False(revokedSecond.IsActive);
        Assert.Equal("reuse_detected", revokedFirst.RevocationReason);
        Assert.Equal("reuse_detected", revokedSecond.RevocationReason);
        Assert.True(activeOther.IsActive);
    }

    [Fact]
    public async Task GetByHash_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IRefreshTokenRepository repository =
            scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        RefreshToken? result = await repository.GetByHashAsync(
            $"missing-{Guid.NewGuid():N}");

        Assert.Null(result);
    }
}
