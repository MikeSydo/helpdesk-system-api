using HelpDesk.Application.Auth;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using Moq;

namespace HelpDesk.Tests.Application;

public class UserAccountDeletionTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly UserManagementService _service;

    public UserAccountDeletionTests()
    {
        _service = new UserManagementService(_userRepository.Object, _refreshTokenRepository.Object);
    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldDeactivateUserAndRevokeRefreshTokens()
    {
        User user = User.Create("User", "user@example.com", "hash");
        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        bool result = await _service.DeleteAccountAsync(user.Id);

        Assert.True(result);
        Assert.False(user.IsActive);
        _userRepository.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepository.Verify(
            x => x.RevokeForUserAsync(
                user.Id,
                "user_deactivated",
                It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenUserDoesNotExist_ShouldNotRevokeTokens()
    {
        Guid userId = Guid.NewGuid();
        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        bool result = await _service.DeleteAccountAsync(userId);

        Assert.False(result);
        _refreshTokenRepository.Verify(
            x => x.RevokeForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _refreshTokenRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenAlreadyInactive_ShouldRemainIdempotent()
    {
        User user = User.Create("User", "user@example.com", "hash");
        user.Deactivate();
        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        bool result = await _service.DeleteAccountAsync(user.Id);

        Assert.True(result);
        Assert.False(user.IsActive);
        _refreshTokenRepository.Verify(
            x => x.RevokeForUserAsync(
                user.Id,
                "user_deactivated",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
