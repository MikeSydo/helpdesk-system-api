namespace HelpDesk.Application.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    Guid UserId,
    string Name,
    string Email,
    string Role);
