using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Tests.Domain;

public sealed class OneTimeTokenTests
{
    [Fact]
    public void MarkUsed_WhenCalled_ShouldMakeTokenInactive()
    {
        OneTimeToken token = OneTimeToken.Create(
            Guid.NewGuid(),
            OneTimeTokenType.PasswordReset,
            "hashed-token",
            DateTime.UtcNow.AddHours(1));

        token.MarkUsed();

        Assert.False(token.IsActive);
    }

    [Fact]
    public void Create_WhenExpirationIsNotLaterThanCreation_ShouldThrow()
    {
        DateTime createdAt = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            OneTimeToken.Create(
                Guid.NewGuid(),
                OneTimeTokenType.EmailConfirmation,
                "hashed-token",
                createdAt,
                createdAt));
    }
}
