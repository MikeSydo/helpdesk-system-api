using HelpDesk.Application.Auth;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.DataAccess.Repositories;

public sealed class OneTimeTokenRepository(AppDbContext dbContext) : IOneTimeTokenRepository
{
    public async Task AddAsync(
        OneTimeToken token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        await dbContext.OneTimeTokens.AddAsync(token, cancellationToken);
    }

    public Task<OneTimeToken?> GetActiveAsync(
        string tokenHash,
        OneTimeTokenType type,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        string normalizedHash = tokenHash.Trim();

        return dbContext.OneTimeTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == normalizedHash &&
                         token.Type == type &&
                         token.UsedAt == null &&
                         token.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task InvalidateActiveAsync(
        Guid userId,
        OneTimeTokenType type,
        CancellationToken cancellationToken = default)
    {
        List<OneTimeToken> tokens = await dbContext.OneTimeTokens
            .Where(token => token.UserId == userId &&
                            token.Type == type &&
                            token.UsedAt == null &&
                            token.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (OneTimeToken token in tokens)
        {
            token.MarkUsed();
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
