namespace HelpDesk.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public Guid TokenFamilyId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
    public Guid? ReplacedByTokenId { get; private set; }
    public string? RevocationReason { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken(Guid userId, string tokenHash, Guid tokenFamilyId,
        DateTime createdAt, DateTime expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        TokenFamilyId = tokenFamilyId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }


    public static RefreshToken Create(Guid userId, string tokenHash, Guid tokenFamilyId, 
        DateTime expiresAt, DateTime? createdAt = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (tokenFamilyId == Guid.Empty)
        {
            throw new ArgumentException("Token family is required.", nameof(tokenFamilyId));
        }

        DateTime issuedAt = createdAt ?? DateTime.UtcNow;
        if (expiresAt <= issuedAt)
        {
            throw new ArgumentException(
                "Token expiration must be later than its creation time.", 
                nameof(expiresAt));
        }

        return new RefreshToken(userId, tokenHash.Trim(), tokenFamilyId, issuedAt, expiresAt);
    }

    public void Revoke(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason.Trim();
    }

    public void MarkAsReplaced(Guid replacementTokenId)
    {
        if (replacementTokenId == Guid.Empty)
        {
            throw new ArgumentException("Replacement token is required.", nameof(replacementTokenId));
        }
        if (replacementTokenId == Id)
        {
            throw new ArgumentException("A token cannot replace itself.", nameof(replacementTokenId));
        }
        if (ReplacedByTokenId is not null && ReplacedByTokenId != replacementTokenId)
        {
            throw new InvalidOperationException("The refresh token has already been replaced.");
        }

        ReplacedByTokenId = replacementTokenId;
        Revoke("rotation");
    }
}
