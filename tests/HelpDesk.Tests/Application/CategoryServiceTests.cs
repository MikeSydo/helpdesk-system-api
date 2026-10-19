using HelpDesk.Application.Categories;
using HelpDesk.Application.Categories.Dtos;
using HelpDesk.Domain.Entities;
using Moq;

namespace HelpDesk.Tests.Application;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _categoryService = new CategoryService(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategoryResponse()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware", "All hardware issues");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        CategoryResponse? result = await _categoryService.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        CategoryResponse? result = await _categoryService.GetByIdAsync(id);

        Assert.Null(result);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_WhenCategoriesExist_ShouldReturnMappedResponses()
    {
        List<Category> categories =
        [
            Category.Create("Hardware", "Hardware issues"),
            Category.Create("Software", "Software issues")
        ];

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        IReadOnlyList<CategoryResponse> result = await _categoryService.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(categories[0].Name, result[0].Name);
        Assert.Equal(categories[1].Name, result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCategoriesExist_ShouldReturnEmptyList()
    {
        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        IReadOnlyList<CategoryResponse> result = await _categoryService.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnCategoryResponse()
    {
        CreateCategoryRequest request = new("Hardware", "All hardware issues");

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        CategoryResponse result = await _categoryService.CreateAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCallAddAsync()
    {
        CreateCategoryRequest request = new("Software", null);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _categoryService.CreateAsync(request);

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldPassCorrectCategoryToRepository()
    {
        CreateCategoryRequest request = new("Network", "Networking issues");

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _categoryService.CreateAsync(request);

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Category>(c =>
                    c.Name == request.Name &&
                    c.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowInvalidOperationException()
    {
        CreateCategoryRequest request = new("Hardware", null);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldNotCallAddAsync()
    {
        CreateCategoryRequest request = new("Hardware", null);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.CreateAsync(request));

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_ShouldUpdateAndReturnResponse()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Old Name", "Old description");
        UpdateCategoryRequest request = new("New Name", "New description");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        CategoryResponse? result = await _categoryService.UpdateAsync(id, request);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New description", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_ShouldCallUpdateAsync()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Old Name");
        UpdateCategoryRequest request = new("New Name", null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                request.Name,
                It.IsAny<CancellationToken>(),
                category.Id))
            .ReturnsAsync(false);

        await _categoryService.UpdateAsync(id, request);

        _categoryRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.Is<Category>(c =>
                    c.Id == category.Id &&
                    c.Name == request.Name &&
                    c.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        Guid id = Guid.NewGuid();
        UpdateCategoryRequest request = new("New Name", null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        CategoryResponse? result = await _categoryService.UpdateAsync(id, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldNotCallSaveChangesAsync()
    {
        Guid id = Guid.NewGuid();
        UpdateCategoryRequest request = new("New Name", null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _categoryService.UpdateAsync(id, request);

        _categoryRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameAlreadyExists_ShouldThrowInvalidOperationException()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware");
        UpdateCategoryRequest request = new("Software", null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                request.Name,
                It.IsAny<CancellationToken>(),
                category.Id))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.UpdateAsync(id, request));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameAlreadyExists_ShouldNotCallSaveChangesAsync()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware");
        UpdateCategoryRequest request = new("Software", null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(
                request.Name,
                It.IsAny<CancellationToken>(),
                category.Id))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.UpdateAsync(id, request));

        _categoryRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameUnchanged_ShouldNotCheckNameDuplicate()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware", "Old description");
        UpdateCategoryRequest request = new("Hardware", "New description");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _categoryService.UpdateAsync(id, request);

        _categoryRepositoryMock.Verify(
            x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_ShouldReturnTrue()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        bool result = await _categoryService.DeleteAsync(id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_ShouldCallDeleteAsync()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _categoryService.DeleteAsync(id);

        _categoryRepositoryMock.Verify(
            x => x.DeleteAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasTickets_ShouldThrowInvalidOperationException()
    {
        Guid id = Guid.NewGuid();
        Category category = Category.Create("Hardware");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock
            .Setup(x => x.HasTicketsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _categoryService.DeleteAsync(id));

        _categoryRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryDoesNotExist_ShouldReturnFalse()
    {
        Guid id = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        bool result = await _categoryService.DeleteAsync(id);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryDoesNotExist_ShouldNotCallDeleteAsync()
    {
        Guid id = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _categoryService.DeleteAsync(id);

        _categoryRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
