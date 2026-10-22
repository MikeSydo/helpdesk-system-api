using HelpDesk.Application.Auth;
using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.DataAccess.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        string normalizedHash = tokenHash.Trim();

        return dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(
                token => token.TokenHash == normalizedHash,
                cancellationToken);
    }

    public async Task RevokeAsync(Guid tokenId, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        RefreshToken? refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.Id == tokenId, cancellationToken);
        
        if (refreshToken is null)
        {
            return;
        }

        refreshToken.Revoke(reason);
    }

    public async Task RevokeFamilyAsync(Guid tokenFamilyId, string reason, CancellationToken cancellationToken = default)
    {
        if (tokenFamilyId == Guid.Empty)
        {
            throw new ArgumentException("Token family is required.", nameof(tokenFamilyId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        List<RefreshToken> tokens = await dbContext.RefreshTokens
            .Where(token => token.TokenFamilyId == tokenFamilyId)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in tokens)
        {
            token.Revoke(reason);
        }
    }

    public async Task MarkAsReplacedAsync(Guid tokenId, Guid replacementTokenId, CancellationToken cancellationToken = default)
    {
        RefreshToken? refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.Id == tokenId, cancellationToken);

        if (refreshToken is null)
        {
            throw new KeyNotFoundException("Refresh token was not found.");
        }

        refreshToken.MarkAsReplaced(replacementTokenId);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
