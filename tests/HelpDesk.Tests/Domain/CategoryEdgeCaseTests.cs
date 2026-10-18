using HelpDesk.Domain.Entities;
// ReSharper disable ConvertToLocalFunction

namespace HelpDesk.Tests.Domain;

public class CategoryEdgeCaseTests
{
    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        Action act = () => Category.Create("");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        Action act = () => Category.Create("   ");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentNullException()
    {
        Action act = () => Category.Create(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        Category category = Category.Create("Hardware");

        Action act = () => category.Update("", null);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithWhitespaceName_ShouldThrowArgumentException()
    {
        Category category = Category.Create("Hardware");

        Action act = () => category.Update("   ", null);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithNullName_ShouldThrowArgumentNullException()
    {
        Category category = Category.Create("Hardware");

        Action act = () => category.Update(null!, null);

        Assert.Throws<ArgumentNullException>(act);
    }
}
