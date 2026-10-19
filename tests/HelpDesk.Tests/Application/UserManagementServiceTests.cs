using HelpDesk.Application.Users;
using HelpDesk.Application.Users.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Moq;

namespace HelpDesk.Tests.Application;

public class UserManagementServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly UserManagementService _userManagementService;

    public UserManagementServiceTests()
    {
        _userManagementService = new UserManagementService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldMapUserWithoutPasswordHash()
    {
        User user = User.Create("Name", "email@example.com", "secret-hash");
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserResponse? result = await _userManagementService.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Role.ToString(), result.Role);
        Assert.Equal(user.CreatedAt, result.CreatedAt);
        Assert.Equal(user.UpdatedAt, result.UpdatedAt);
        Assert.Equal(user.IsActive, result.IsActive);
        Assert.DoesNotContain("secret-hash", result.ToString());
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserResponse? result = await _userManagementService.GetByIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenUsersDoNotExist_ShouldReturnEmptyList()
    {
        _userRepositoryMock
            .Setup(x => x.GetAllUsers(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<User>());

        IReadOnlyList<UserResponse> result = await _userManagementService.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ChangeRoleAsync_WhenUserExists_ShouldUpdateRoleAndPersist()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        ChangeRoleRequest request = new(UserRole.Support);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        UserResponse? result =
            await _userManagementService.ChangeRoleAsync(user.Id, request);

        Assert.NotNull(result);
        Assert.Equal(UserRole.Support, user.Role);
        Assert.Equal(nameof(UserRole.Support), result.Role);
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeRoleAsync_WhenUserDoesNotExist_ShouldReturnNullAndNotPersist()
    {
        Guid id = Guid.NewGuid();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserResponse? result =
            await _userManagementService.ChangeRoleAsync(id, new ChangeRoleRequest(UserRole.Admin));

        Assert.Null(result);
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_WhenUserExists_ShouldDeactivateAndPersist()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        bool result = await _userManagementService.DeactivateAsync(user.Id);

        Assert.True(result);
        Assert.False(user.IsActive);
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenUserDoesNotExist_ShouldReturnFalseAndNotPersist()
    {
        Guid id = Guid.NewGuid();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        bool result = await _userManagementService.DeactivateAsync(id);

        Assert.False(result);
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
