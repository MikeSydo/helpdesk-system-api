using HelpDesk.Domain.Entities;

namespace HelpDesk.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithNameOnly_ShouldCreateCategory()
    {
        Category category = Category.Create("Hardware");

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Hardware", category.Name);
        Assert.Null(category.Description);
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldSetBothFields()
    {
        Category category = Category.Create("Software", "All software issues");

        Assert.Equal("Software", category.Name);
        Assert.Equal("All software issues", category.Description);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        Category category = Category.Create("  Support  ");

        Assert.Equal("Support", category.Name);
    }

    [Fact]
    public void Create_ShouldTrimDescription()
    {
        Category category = Category.Create("Network", "  Networking issues  ");

        Assert.Equal("Networking issues", category.Description);
    }

    [Fact]
    public void Create_ShouldInitializeEmptyTicketsList()
    {
        Category category = Category.Create("Hardware");

        Assert.NotNull(category.Tickets);
        Assert.Empty(category.Tickets);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        Category category1 = Category.Create("Hardware");
        Category category2 = Category.Create("Software");

        Assert.NotEqual(category1.Id, category2.Id);
    }

    [Fact]
    public void Update_WhenCalled_ShouldChangeName()
    {
        Category category = Category.Create("Old Name");

        category.Update("New Name", null);

        Assert.Equal("New Name", category.Name);
    }

    [Fact]
    public void Update_WhenCalled_ShouldChangeDescription()
    {
        Category category = Category.Create("Hardware", "Old description");

        category.Update("Hardware", "New description");

        Assert.Equal("New description", category.Description);
    }

    [Fact]
    public void Update_WhenCalledWithNullDescription_ShouldSetDescriptionToNull()
    {
        Category category = Category.Create("Hardware", "Some description");

        category.Update("Hardware", null);

        Assert.Null(category.Description);
    }

    [Fact]
    public void Update_ShouldTrimName()
    {
        Category category = Category.Create("Hardware");

        category.Update("  New Name  ", null);

        Assert.Equal("New Name", category.Name);
    }

    [Fact]
    public void Update_ShouldTrimDescription()
    {
        Category category = Category.Create("Hardware");

        category.Update("Hardware", "  New description  ");

        Assert.Equal("New description", category.Description);
    }
}
