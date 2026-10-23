using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Auth;

public interface IOneTimeTokenRepository
{
    Task AddAsync(OneTimeToken token, CancellationToken cancellationToken = default);
    Task<OneTimeToken?> GetActiveAsync(string tokenHash, OneTimeTokenType type, 
        CancellationToken cancellationToken = default);
    Task InvalidateActiveAsync(Guid userId, OneTimeTokenType type, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
