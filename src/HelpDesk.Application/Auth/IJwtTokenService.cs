using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Auth;

public interface IJwtTokenService
{
    Task<AuthTokenResult> IssueAsync(User user, CancellationToken cancellationToken = default);
    Task<AuthTokenResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}
