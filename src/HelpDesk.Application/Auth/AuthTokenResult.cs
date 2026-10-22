using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Auth;

public sealed record AuthTokenResult(
    User User,
    string AccessToken,
    string RefreshToken,
    Guid RefreshTokenId,
    DateTime AccessTokenExpiresAt);
