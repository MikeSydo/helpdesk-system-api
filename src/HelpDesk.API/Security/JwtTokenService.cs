using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HelpDesk.Application.Auth;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.API.Security;

public sealed class JwtTokenService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<AuthTokenResult> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ValidateOptions();

        AuthTokenResult result = CreateTokenResult(user);

        await refreshTokenRepository.AddAsync(result.StoredRefreshToken!, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthTokenResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ValidateOptions();

        RefreshToken? storedToken = await refreshTokenRepository.GetByHashAsync(
            HashRefreshToken(refreshToken),
            cancellationToken);

        if (storedToken is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (!storedToken.IsActive)
        {
            await refreshTokenRepository.RevokeFamilyAsync(
                storedToken.TokenFamilyId,
                "reuse_detected",
                cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        User? user = await userRepository.GetByIdAsync(
            storedToken.UserId,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        AuthTokenResult replacement = CreateTokenResult(user);
        bool rotated = await refreshTokenRepository.RotateAsync(
            HashRefreshToken(refreshToken),
            replacement.StoredRefreshToken!,
            cancellationToken);

        if (!rotated)
        {
            await refreshTokenRepository.RevokeFamilyAsync(
                storedToken.TokenFamilyId,
                "reuse_detected",
                cancellationToken);
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        return replacement;
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        RefreshToken? storedToken = await refreshTokenRepository.GetByHashAsync(
            HashRefreshToken(refreshToken),
            cancellationToken);

        if (storedToken is null)
        {
            return;
        }

        await refreshTokenRepository.RevokeAsync(storedToken.Id, "logout", cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private string CreateAccessToken(User user, DateTime now, DateTime expiresAt)
    {
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        ];

        JwtSecurityToken token = new(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            expiresAt,
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private AuthTokenResult CreateTokenResult(User user)
    {
        DateTime now = DateTime.UtcNow;
        DateTime accessTokenExpiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        string accessToken = CreateAccessToken(user, now, accessTokenExpiresAt);
        string rawRefreshToken = CreateRawRefreshToken();
        RefreshToken refreshToken = RefreshToken.Create(
            user.Id,
            HashRefreshToken(rawRefreshToken),
            Guid.NewGuid(),
            now.AddDays(_options.RefreshTokenDays),
            now);

        return new AuthTokenResult(
            user,
            accessToken,
            rawRefreshToken,
            refreshToken.Id,
            accessTokenExpiresAt,
            refreshToken);
    }

    private static string CreateRawRefreshToken() => 
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer) ||
            string.IsNullOrWhiteSpace(_options.Audience) ||
            string.IsNullOrWhiteSpace(_options.SigningKey) ||
            _options.SigningKey.Length < 32 ||
            _options.AccessTokenMinutes <= 0 ||
            _options.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("JWT configuration is invalid.");
        }
    }

}
