using HelpDesk.Domain.Entities;

namespace HelpDesk.Tests.Domain;

public class UserEdgeCaseTests
{
    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        Action act = () => User.Create("", "email@example.com", "hash");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        Action act = () => User.Create("   ", "email@example.com", "hash");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentNullException()
    {
        Action act = () => User.Create(null!, "email@example.com", "hash");

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException()
    {
        Action act = () => User.Create("Name", "", "hash");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithWhitespaceEmail_ShouldThrowArgumentException()
    {
        Action act = () => User.Create("Name", "   ", "hash");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrowArgumentNullException()
    {
        Action act = () => User.Create("Name", null!, "hash");

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_ShouldThrowArgumentException()
    {
        Action act = () => User.Create("Name", "email@example.com", "");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithWhitespacePasswordHash_ShouldThrowArgumentException()
    {
        Action act = () => User.Create("Name", "email@example.com", "   ");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        Action act = () => User.Create("Name", "email@example.com", null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowArgumentException()
    {
        User user = User.Create("Name", "email@example.com", "hash");

        Action act = () => user.UpdateName("");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateEmail_WithWhitespaceEmail_ShouldThrowArgumentException()
    {
        User user = User.Create("Name", "email@example.com", "hash");

        Action act = () => user.UpdateEmail("   ");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdatePasswordHash_WithNullHash_ShouldThrowArgumentNullException()
    {
        User user = User.Create("Name", "email@example.com", "hash");

        Action act = () => user.UpdatePasswordHash(null!);

        Assert.Throws<ArgumentNullException>(act);
    }
}
