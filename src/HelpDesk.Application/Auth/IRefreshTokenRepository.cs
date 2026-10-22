using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid tokenId, string reason, CancellationToken cancellationToken = default);
    Task RevokeFamilyAsync(Guid tokenFamilyId, string reason, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
