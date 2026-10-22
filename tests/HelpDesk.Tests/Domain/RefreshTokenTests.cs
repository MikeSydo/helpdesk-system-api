using HelpDesk.Domain.Entities;

namespace HelpDesk.Tests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeActiveToken()
    {
        Guid userId = Guid.NewGuid();
        Guid familyId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;
        DateTime expiresAt = createdAt.AddDays(30);

        RefreshToken token = RefreshToken.Create(
            userId,
            " hashed-token ",
            familyId,
            expiresAt,
            createdAt);

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("hashed-token", token.TokenHash);
        Assert.Equal(familyId, token.TokenFamilyId);
        Assert.Equal(createdAt, token.CreatedAt);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.True(token.IsActive);
        Assert.Null(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenId);
        Assert.Null(token.RevocationReason);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTokenHash_ShouldThrow(string tokenHash)
    {
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                Guid.NewGuid(),
                tokenHash,
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                Guid.Empty,
                "hash",
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Create_WithEmptyFamilyId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                Guid.NewGuid(),
                "hash",
                Guid.Empty,
                DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Create_WhenExpirationIsNotAfterCreation_ShouldThrow()
    {
        DateTime createdAt = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create(
                Guid.NewGuid(),
                "hash",
                Guid.NewGuid(),
                createdAt,
                createdAt));
    }

    [Fact]
    public void ExpiredToken_ShouldNotBeActive()
    {
        DateTime createdAt = DateTime.UtcNow.AddDays(-2);
        RefreshToken token = RefreshToken.Create(
            Guid.NewGuid(),
            "hash",
            Guid.NewGuid(),
            createdAt.AddDays(1),
            createdAt);

        Assert.False(token.IsActive);
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldDeactivateTokenAndStoreReason()
    {
        RefreshToken token = CreateToken();

        token.Revoke(" logout ");

        Assert.False(token.IsActive);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("logout", token.RevocationReason);
    }

    [Fact]
    public void Revoke_WhenCalledAgain_ShouldPreserveOriginalRevocation()
    {
        RefreshToken token = CreateToken();
        token.Revoke("logout");
        DateTime? revokedAt = token.RevokedAt;

        token.Revoke("reuse_detected");

        Assert.Equal(revokedAt, token.RevokedAt);
        Assert.Equal("logout", token.RevocationReason);
    }

    [Fact]
    public void MarkAsReplaced_ShouldRevokeWithRotationReason()
    {
        RefreshToken token = CreateToken();
        Guid replacementId = Guid.NewGuid();

        token.MarkAsReplaced(replacementId);

        Assert.False(token.IsActive);
        Assert.Equal(replacementId, token.ReplacedByTokenId);
        Assert.Equal("rotation", token.RevocationReason);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void MarkAsReplaced_WithEmptyReplacementId_ShouldThrow()
    {
        RefreshToken token = CreateToken();

        Assert.Throws<ArgumentException>(() => token.MarkAsReplaced(Guid.Empty));
    }

    [Fact]
    public void MarkAsReplaced_WithOwnId_ShouldThrow()
    {
        RefreshToken token = CreateToken();

        Assert.Throws<ArgumentException>(() => token.MarkAsReplaced(token.Id));
    }

    [Fact]
    public void MarkAsReplaced_WithDifferentReplacementAfterRotation_ShouldThrow()
    {
        RefreshToken token = CreateToken();
        token.MarkAsReplaced(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            token.MarkAsReplaced(Guid.NewGuid()));
    }

    private static RefreshToken CreateToken()
    {
        DateTime createdAt = DateTime.UtcNow;
        return RefreshToken.Create(
            Guid.NewGuid(),
            "hash",
            Guid.NewGuid(),
            createdAt.AddDays(30),
            createdAt);
    }
}
