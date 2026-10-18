using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Tests.Domain;

public class TicketEdgeCaseTests
{
    [Fact]
    public void Create_WithEmptyCategoryId_ShouldThrowArgumentException()
    {
        Action act = () => Ticket.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "VPN issue",
            "Cannot connect to VPN",
            TicketPriority.High);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithEmptyCreatedByUserId_ShouldThrowArgumentException()
    {
        Action act = () => Ticket.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "VPN issue",
            "Cannot connect to VPN",
            TicketPriority.High);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        Action act = () => Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            "Cannot connect to VPN",
            TicketPriority.High);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ShouldThrowArgumentException()
    {
        Action act = () => Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "   ",
            "Cannot connect to VPN",
            TicketPriority.High);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldThrowArgumentException()
    {
        Action act = () => Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VPN issue",
            "",
            TicketPriority.High);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithWhitespaceDescription_ShouldThrowArgumentException()
    {
        Action act = () => Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VPN issue",
            "   ",
            TicketPriority.High);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ShouldThrowArgumentException()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title",
            "Old description");

        Action act = () => ticket.UpdateDetails("", "New description");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateDetails_WithWhitespaceTitle_ShouldThrowArgumentException()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title",
            "Old description");

        Action act = () => ticket.UpdateDetails("   ", "New description");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateDetails_WithEmptyDescription_ShouldThrowArgumentException()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title",
            "Old description");

        Action act = () => ticket.UpdateDetails("New title", "");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void UpdateDetails_WithWhitespaceDescription_ShouldThrowArgumentException()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title",
            "Old description");

        Action act = () => ticket.UpdateDetails("New title", "   ");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void AssignTo_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            TicketPriority.Low);

        Action act = () => ticket.AssignTo(Guid.Empty);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ChangePriority_ToSameValue_ShouldKeepPriority()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        ticket.ChangePriority(TicketPriority.Medium);

        Assert.Equal(TicketPriority.Medium, ticket.Priority);
    }

    [Fact]
    public void ChangeStatus_ToSameValue_ShouldKeepStatus()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        ticket.ChangeStatus(TicketStatus.Open);

        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void ChangeCategory_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        Action act = () => ticket.ChangeCategory(Guid.Empty);

        Assert.Throws<ArgumentException>(act);
    }
}