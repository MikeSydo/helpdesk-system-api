using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Moq;

namespace HelpDesk.Tests.Application;

public class TicketProcessingServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly TicketProcessingService _service;

    public TicketProcessingServiceTests()
    {
        _service = new TicketProcessingService(
            _ticketRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTicketExists_ShouldUpdateAndPersist()
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

        TicketResponse? result = await _service.ChangeStatusAsync(
            ticketId,
            new ChangeTicketStatusRequest(TicketStatus.InProgress));

        Assert.NotNull(result);
        Assert.Equal(TicketStatus.InProgress, result.Status);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTicketDoesNotExist_ShouldReturnNull()
    {
        Guid ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        TicketResponse? result = await _service.ChangeStatusAsync(
            ticketId,
            new ChangeTicketStatusRequest(TicketStatus.InProgress));

        Assert.Null(result);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignSupportAsync_WhenUserExists_ShouldAssignAndPersist()
    {
        Guid ticketId = Guid.NewGuid();
        Guid supportUserId = Guid.NewGuid();
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");
        User user = User.Create("Support", "support@example.com", "hash");

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(supportUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        TicketResponse? result = await _service.AssignSupportAsync(
            ticketId,
            new AssignTicketRequest(supportUserId));

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.AssignedToSupportId);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AssignSupportAsync_WhenUserDoesNotExist_ShouldThrow()
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
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(supportUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AssignSupportAsync(
                ticketId,
                new AssignTicketRequest(supportUserId)));

        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UnassignSupportAsync_WhenTicketExists_ShouldClearAssignmentAndPersist()
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

        TicketResponse? result = await _service.UnassignSupportAsync(ticketId);

        Assert.NotNull(result);
        Assert.Null(result.AssignedToSupportId);
        _ticketRepositoryMock.Verify(
            x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
