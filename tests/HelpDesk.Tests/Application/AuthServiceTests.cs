using HelpDesk.Application.Auth;
using HelpDesk.Application.Auth.Dtos;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Moq;

namespace HelpDesk.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IOneTimeTokenRepository> _oneTimeTokenRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _userRepository.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _oneTimeTokenRepository.Object,
            _refreshTokenRepository.Object,
            _emailSender.Object,
            new EmailOptions
            {
                ConfirmationLinkBaseUrl = "https://example.test/confirm",
                PasswordResetLinkBaseUrl = "https://example.test/reset"
            });
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUnconfirmedUserWithoutTokens()
    {
        const string password = "Password1";
        const string passwordHash = "hashed-password";
        RegisterRequest request = new(
            "New User",
            "new@example.com",
            password,
            password);

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(
                "new@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher
            .Setup(x => x.Hash(It.IsAny<User>(), password))
            .Returns(passwordHash);
        _emailSender
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        PendingRegistrationResponse result = await _service.RegisterAsync(request);

        Assert.Equal("new@example.com", result.Email);
        Assert.False(GetAddedUser().EmailConfirmed);
        Assert.Equal(passwordHash, GetAddedUser().PasswordHash);
        _jwtTokenService.Verify(
            x => x.IssueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ShouldThrowConflict()
    {
        User existingUser = User.Create(
            "Existing",
            "existing@example.com",
            "hash");
        _userRepository
            .Setup(x => x.GetUserByEmailAsync(
                existingUser.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(new RegisterRequest(
                "New User",
                "existing@example.com",
                "Password1",
                "Password1")));

        _userRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldIssueTokens()
    {
        User user = User.Create("User", "user@example.com", "stored-hash");
        user.ConfirmEmail();
        _userRepository
            .Setup(x => x.GetUserByEmailAsync(
                user.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.Verify(user, "Password1", user.PasswordHash))
            .Returns(true);
        _jwtTokenService
            .Setup(x => x.IssueAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTokens(user));

        AuthResponse result = await _service.LoginAsync(
            new LoginRequest(user.Email, "Password1"));

        Assert.Equal(user.Id, result.UserId);
        _jwtTokenService.Verify(
            x => x.IssueAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldUseGenericUnauthorizedError()
    {
        _userRepository
            .Setup(x => x.GetUserByEmailAsync(
                "missing@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UnauthorizedException exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest(
                "missing@example.com",
                "Password1")));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ShouldRejectCredentials()
    {
        User user = User.Create("User", "inactive@example.com", "stored-hash");
        user.Deactivate();
        _userRepository
            .Setup(x => x.GetUserByEmailAsync(
                user.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest(
                user.Email,
                "Password1")));

        _passwordHasher.Verify(
            x => x.Verify(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsNotConfirmed_ShouldRejectCredentials()
    {
        User user = User.Create("User", "unconfirmed@example.com", "stored-hash");
        _userRepository
            .Setup(x => x.GetUserByEmailAsync(
                user.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest(user.Email, "Password1")));

        _jwtTokenService.Verify(
            x => x.IssueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldConfirmUserAndConsumeToken()
    {
        User user = User.Create("User", "confirm@example.com", "stored-hash");
        OneTimeToken token = OneTimeToken.Create(
            user.Id,
            OneTimeTokenType.EmailConfirmation,
            "hashed-token",
            DateTime.UtcNow.AddHours(1));
        _oneTimeTokenRepository
            .Setup(x => x.GetActiveAsync(
                It.IsAny<string>(),
                OneTimeTokenType.EmailConfirmation,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _service.ConfirmEmailAsync(new ConfirmEmailRequest("raw-token"));

        Assert.True(user.EmailConfirmed);
        Assert.False(token.IsActive);
        _userRepository.Verify(
            x => x.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldUpdateHashAndRevokeRefreshTokens()
    {
        User user = User.Create("User", "reset@example.com", "old-hash");
        user.ConfirmEmail();
        OneTimeToken token = OneTimeToken.Create(
            user.Id,
            OneTimeTokenType.PasswordReset,
            "hashed-token",
            DateTime.UtcNow.AddHours(1));
        _oneTimeTokenRepository
            .Setup(x => x.GetActiveAsync(
                It.IsAny<string>(),
                OneTimeTokenType.PasswordReset,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _userRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.Hash(user, "NewPassword1"))
            .Returns("new-hash");

        await _service.ResetPasswordAsync(
            new ResetPasswordRequest("raw-token", "NewPassword1", "NewPassword1"));

        Assert.Equal("new-hash", user.PasswordHash);
        _refreshTokenRepository.Verify(
            x => x.RevokeForUserAsync(
                user.Id,
                "password_reset",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenPasswordsDoNotMatch_ShouldRejectBeforePersistence()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(new RegisterRequest(
                "User",
                "user@example.com",
                "Password1",
                "Password2")));

        _userRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private User GetAddedUser() =>
        _userRepository
            .Invocations
            .Single(x => x.Method.Name == nameof(IUserRepository.AddAsync))
            .Arguments[0] as User
        ?? throw new InvalidOperationException("User was not added.");

    private static AuthTokenResult CreateTokens(User user) =>
        new(
            user,
            "access-token",
            "refresh-token",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(15));
}
