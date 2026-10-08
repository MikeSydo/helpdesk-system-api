using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Moq;

namespace HelpDesk.Tests.Application;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly TicketService _ticketService;

    public TicketServiceTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _ticketService = new TicketService(_ticketRepositoryMock.Object);
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
            Guid.NewGuid(),
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
    public async Task UpdateAsync_WhenTicketExists_ShouldCallSaveChangesAsync()
    {
        Guid ticketId = Guid.NewGuid();
        Guid currentUserId = Guid.NewGuid();

        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
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
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
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
    public async Task UpdateAsync_WhenTicketDoesNotExist_ShouldNotCallSaveChangesAsync()
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
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
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
}