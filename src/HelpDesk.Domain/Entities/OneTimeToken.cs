using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class OneTimeToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public OneTimeTokenType Type { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public User User { get; private set; } = null!;
    public bool IsActive => UsedAt is null && ExpiresAt > DateTime.UtcNow;

    private OneTimeToken(
        Guid userId,
        OneTimeTokenType type,
        string tokenHash,
        DateTime createdAt,
        DateTime expiresAt)
    {
        UserId = userId;
        Type = type;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static OneTimeToken Create(
        Guid userId,
        OneTimeTokenType type,
        string tokenHash,
        DateTime expiresAt,
        DateTime? createdAt = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        DateTime issuedAt = createdAt ?? DateTime.UtcNow;

        if (expiresAt <= issuedAt)
        {
            throw new ArgumentException(
                "Token expiration must be later than its creation time.",
                nameof(expiresAt));
        }

        return new OneTimeToken(
            userId,
            type,
            tokenHash.Trim(),
            issuedAt,
            expiresAt);
    }

    public void MarkUsed()
    {
        if (UsedAt is not null)
        {
            throw new InvalidOperationException("The token has already been used.");
        }

        UsedAt = DateTime.UtcNow;
    }
}
