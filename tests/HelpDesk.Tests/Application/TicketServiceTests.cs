using HelpDesk.Application.Categories;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Moq;

namespace HelpDesk.Tests.Application;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly TicketService _ticketService;

    public TicketServiceTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _ticketService = new TicketService(_ticketRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketExists_ShouldReturnTicketResponse()
    {
        Guid ticketId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VPN issue",
            "Cannot connect to VPN",
            TicketPriority.High);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        TicketResponse? result = await _ticketService.GetByIdAsync(ticketId);

        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Title, result.Title);
        Assert.Equal(ticket.Description, result.Description);
        Assert.Equal(ticket.Status, result.Status);
        Assert.Equal(ticket.Priority, result.Priority);
        Assert.Equal(ticket.CategoryId, result.CategoryId);
        Assert.Equal(ticket.CreatedByUserId, result.CreatedByUserId);
        Assert.Equal(ticket.AssignedToSupportId, result.AssignedToSupportId);
        Assert.Equal(ticket.CreatedAt, result.CreatedAt);
        Assert.Equal(ticket.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketDoesNotExist_ShouldReturnNull()
    {
        Guid ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        TicketResponse? result = await _ticketService.GetByIdAsync(ticketId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenTicketsExist_ShouldReturnMappedResponses()
    {
        List<Ticket> tickets =
        [
            Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Issue 1", "Description 1", TicketPriority.Low),
            Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Issue 2", "Description 2", TicketPriority.High)
        ];

        _ticketRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        IReadOnlyList<TicketResponse> result = await _ticketService.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(tickets[0].Id, result[0].Id);
        Assert.Equal(tickets[0].Title, result[0].Title);
        Assert.Equal(tickets[1].Id, result[1].Id);
        Assert.Equal(tickets[1].Title, result[1].Title);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoTicketsExist_ShouldReturnEmptyList()
    {
        _ticketRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ticket>());

        IReadOnlyList<TicketResponse> result = await _ticketService.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnTicketResponse()
    {
        Guid currentUserId = Guid.NewGuid();

        CreateTicketRequest request = new(
            "Email issue",
            "Emails are not sending",
            Guid.NewGuid(),
            TicketPriority.Medium);

        Category category = Category.Create("Category 1");
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        TicketResponse result = await _ticketService.CreateAsync(request, currentUserId);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.CategoryId, result.CategoryId);
        Assert.Equal(request.Priority, result.Priority);
        Assert.Equal(currentUserId, result.CreatedByUserId);
        Assert.Equal(TicketStatus.Open, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCallAddAsync()
    {
        Guid currentUserId = Guid.NewGuid();

        CreateTicketRequest request = new(
            "Printer issue",
            "Printer does not print",
            Guid.NewGuid(),
            TicketPriority.Low);

        Category category = Category.Create("Category 1");
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _ticketService.CreateAsync(request, currentUserId);

        _ticketRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldPassCreatedTicketToRepository()
    {
        Guid currentUserId = Guid.NewGuid();

        CreateTicketRequest request = new(
            "Network issue",
            "Internet is unstable",
            Guid.NewGuid(),
            TicketPriority.High);

        Category category = Category.Create("Category 1");
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _ticketService.CreateAsync(request, currentUserId);

        _ticketRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Ticket>(t =>
                    t.Title == request.Title &&
                    t.Description == request.Description &&
                    t.CategoryId == request.CategoryId &&
                    t.Priority == request.Priority &&
                    t.CreatedByUserId == currentUserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketExists_ShouldUpdateTicketAndReturnResponse()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            currentUserId,
            "Old title",
            "Old description",
            TicketPriority.Low);

        UpdateTicketRequest request = new(
            "New title",
            "New description",
            ticket.CategoryId,
            TicketPriority.High);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        TicketResponse? result = await _ticketService.UpdateAsync(ticketId, request, currentUserId);

        Assert.NotNull(result);
        Assert.Equal("New title", result.Title);
        Assert.Equal("New description", result.Description);
        Assert.Equal(TicketPriority.High, result.Priority);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketExists_ShouldCallUpdateAsync()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            currentUserId,
            "Old title",
            "Old description",
            TicketPriority.Low);

        UpdateTicketRequest request = new(
            "Updated title",
            "Updated description",
            ticket.CategoryId,
            TicketPriority.Medium);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        await _ticketService.UpdateAsync(ticketId, request, currentUserId);

        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketDoesNotExist_ShouldReturnNull()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();

        UpdateTicketRequest request = new(
            "Updated title",
            "Updated description",
            Guid.NewGuid(),
            TicketPriority.Medium);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        TicketResponse? result = await _ticketService.UpdateAsync(ticketId, request, currentUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketDoesNotExist_ShouldNotCallUpdateAsync()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();

        UpdateTicketRequest request = new(
            "Updated title",
            "Updated description",
            Guid.NewGuid(),
            TicketPriority.Medium);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        await _ticketService.UpdateAsync(ticketId, request, currentUserId);

        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketExists_ShouldReturnTrue()
    {
        Guid ticketId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            TicketPriority.Low);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        bool result = await _ticketService.DeleteAsync(ticketId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketExists_ShouldCallDeleteAsync()
    {
        Guid ticketId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            TicketPriority.Low);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        await _ticketService.DeleteAsync(ticketId);

        _ticketRepositoryMock.Verify(
            x => x.DeleteAsync(ticketId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketDoesNotExist_ShouldReturnFalse()
    {
        Guid ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        bool result = await _ticketService.DeleteAsync(ticketId);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketDoesNotExist_ShouldNotCallDeleteAsync()
    {
        Guid ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        await _ticketService.DeleteAsync(ticketId);

        _ticketRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryIdIsEmpty_ShouldThrowArgumentException()
    {
        Guid currentUserId = Guid.NewGuid();
        CreateTicketRequest request = new(
            "Title",
            "Description",
            Guid.Empty,
            TicketPriority.Low);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _ticketService.CreateAsync(request, currentUserId));
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldThrowKeyNotFoundException()
    {
        Guid currentUserId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        CreateTicketRequest request = new(
            "Title",
            "Description",
            categoryId,
            TicketPriority.Low);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _ticketService.CreateAsync(request, currentUserId));
    }

    [Fact]
    public async Task UpdateAsync_WhenUserIsNotAuthor_ShouldThrowUnauthorizedAccessException()
    {
        Guid ticketId = Guid.NewGuid();
        Guid authorId = Guid.NewGuid();
        Guid anotherUserId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            authorId,
            "Old title",
            "Old description",
            TicketPriority.Low);

        UpdateTicketRequest request = new(
            "New title",
            "New description",
            ticket.CategoryId,
            TicketPriority.Medium);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _ticketService.UpdateAsync(ticketId, request, anotherUserId));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryChanges_AndCategoryNotFound_ShouldThrowKeyNotFoundException()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();
        Guid oldCategoryId = Guid.NewGuid();
        Guid newCategoryId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            oldCategoryId,
            currentUserId,
            "Old title",
            "Old description",
            TicketPriority.Low);

        UpdateTicketRequest request = new(
            "New title",
            "New description",
            newCategoryId,
            TicketPriority.Medium);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _ticketService.UpdateAsync(ticketId, request, currentUserId));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryChanges_AndCategoryExists_ShouldUpdateCategory()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();
        Guid oldCategoryId = Guid.NewGuid();
        Guid newCategoryId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            oldCategoryId,
            currentUserId,
            "Old title",
            "Old description",
            TicketPriority.Low);

        UpdateTicketRequest request = new(
            "New title",
            "New description",
            newCategoryId,
            TicketPriority.Medium);

        Category newCategory = Category.Create("New Category");

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        TicketResponse? result = await _ticketService.UpdateAsync(ticketId, request, currentUserId);

        Assert.NotNull(result);
        Assert.Equal(newCategoryId, result.CategoryId);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTicketExists_ShouldUpdateStatusAndReturnResponse()
    {
        Guid ticketId = Guid.NewGuid();
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        TicketResponse? result = await _ticketService.ChangeStatusAsync(ticketId, TicketStatus.InProgress, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(TicketStatus.InProgress, result.Status);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTicketDoesNotExist_ShouldReturnNull()
    {
        Guid ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        TicketResponse? result = await _ticketService.ChangeStatusAsync(ticketId, TicketStatus.InProgress, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AssignSupportAsync_WhenTicketExists_ShouldAssignSupportAndReturnResponse()
    {
        Guid ticketId = Guid.NewGuid();
        Guid supportUserId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        TicketResponse? result = await _ticketService.AssignSupportAsync(ticketId, supportUserId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(supportUserId, result.AssignedToSupportId);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AssignSupportAsync_WhenTicketDoesNotExist_ShouldReturnNull()
    {
        Guid ticketId = Guid.NewGuid();
        Guid supportUserId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        TicketResponse? result = await _ticketService.AssignSupportAsync(ticketId, supportUserId, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UnassignSupportAsync_WhenTicketExists_ShouldUnassignAndReturnResponse()
    {
        Guid ticketId = Guid.NewGuid();
        Guid supportUserId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");
        ticket.AssignTo(supportUserId);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        TicketResponse? result = await _ticketService.UnassignSupportAsync(ticketId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.AssignedToSupportId);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UnassignSupportAsync_WhenTicketDoesNotExist_ShouldReturnNull()
    {
        Guid ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        TicketResponse? result = await _ticketService.UnassignSupportAsync(ticketId, CancellationToken.None);

        Assert.Null(result);
    }
}