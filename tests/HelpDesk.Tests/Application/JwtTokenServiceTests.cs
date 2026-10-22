using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HelpDesk.API.Security;
using HelpDesk.Application.Auth;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;

namespace HelpDesk.Tests.Application;

public class JwtTokenServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _service = new JwtTokenService(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            Options.Create(new JwtOptions
            {
                Issuer = "HelpDesk.Tests",
                Audience = "HelpDesk.Tests.Client",
                SigningKey = "test-signing-key-with-at-least-32-characters",
                AccessTokenMinutes = 15,
                RefreshTokenDays = 30
            }));
    }

    [Fact]
    public async Task IssueAsync_ShouldCreateJwtWithExpectedClaimsAndPersistHashedRefreshToken()
    {
        User user = User.Create(
            "Test User",
            "test@example.com",
            "password-hash",
            UserRole.Support);
        RefreshToken? persistedToken = null;
        _refreshTokenRepository
            .Setup(x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => persistedToken = token)
            .Returns(Task.CompletedTask);

        AuthTokenResult result = await _service.IssueAsync(user);

        Assert.NotNull(persistedToken);
        Assert.NotEqual(result.RefreshToken, persistedToken.TokenHash);
        Assert.Equal(user.Id, persistedToken.UserId);
        Assert.True(persistedToken.IsActive);
        Assert.Equal(result.RefreshTokenId, persistedToken.Id);
        _refreshTokenRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        JwtSecurityToken jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(result.AccessToken);

        Assert.Equal("HelpDesk.Tests", jwt.Issuer);
        Assert.Equal("HelpDesk.Tests.Client", jwt.Audiences.Single());
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(
            claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(nameof(UserRole.Support), jwt.Claims.Single(
            claim => claim.Type == ClaimTypes.Role).Value);
        Assert.Equal(user.Email, jwt.Claims.Single(
            claim => claim.Type == ClaimTypes.Email).Value);
    }

    [Fact]
    public async Task IssueAsync_WithInvalidConfiguration_ShouldThrow()
    {
        JwtTokenService service = new(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            Options.Create(new JwtOptions
            {
                Issuer = "issuer",
                Audience = "audience",
                SigningKey = "short"
            }));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.IssueAsync(User.Create(
                "Test",
                "test@example.com",
                "hash")));
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenDoesNotExist_ShouldReject()
    {
        _refreshTokenRepository
            .Setup(x => x.GetByHashAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.RefreshAsync("missing-refresh-token"));
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsRevoked_ShouldRevokeFamilyAndReject()
    {
        User user = User.Create("Test", "test@example.com", "hash");
        RefreshToken token = RefreshToken.Create(
            user.Id,
            "stored-hash",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30));
        token.Revoke("rotation");
        _refreshTokenRepository
            .Setup(x => x.GetByHashAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.RefreshAsync("reused-refresh-token"));

        _refreshTokenRepository.Verify(
            x => x.RevokeFamilyAsync(
                token.TokenFamilyId,
                "reuse_detected",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenUserIsInactive_ShouldReject()
    {
        User user = User.Create("Test", "test@example.com", "hash");
        user.Deactivate();
        RefreshToken token = RefreshToken.Create(
            user.Id,
            "stored-hash",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30));
        _refreshTokenRepository
            .Setup(x => x.GetByHashAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _userRepository
            .Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.RefreshAsync("refresh-token"));

        _refreshTokenRepository.Verify(
            x => x.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeAsync_WhenTokenExists_ShouldPersistLogoutRevocation()
    {
        User user = User.Create("Test", "test@example.com", "hash");
        RefreshToken token = RefreshToken.Create(
            user.Id,
            "stored-hash",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30));
        _refreshTokenRepository
            .Setup(x => x.GetByHashAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await _service.RevokeAsync("refresh-token");

        _refreshTokenRepository.Verify(
            x => x.RevokeAsync(
                token.Id,
                "logout",
                It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
