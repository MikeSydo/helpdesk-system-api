using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Tests.Domain;

public class TicketTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTicketWithDefaultStatus()
    {
        Guid categoryId = Guid.NewGuid();
        Guid createdByUserId = Guid.NewGuid();
        string title = "VPN is not working";
        string description = "I cannot connect to the corporate VPN.";
        TicketPriority priority = TicketPriority.High;

        Ticket ticket = Ticket.Create(categoryId, createdByUserId, title, description, priority);

        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(categoryId, ticket.CategoryId);
        Assert.Equal(createdByUserId, ticket.CreatedByUserId);
        Assert.Equal(title, ticket.Title);
        Assert.Equal(description, ticket.Description);
        Assert.Equal(priority, ticket.Priority);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.NotEqual(default, ticket.CreatedAt);
    }
    
    [Fact]
    public void UpdateDetails_WhenCalled_ShouldChangeTitleAndDescription()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title",
            "Old description");

        ticket.UpdateDetails("New title", "New description");

        Assert.Equal("New title", ticket.Title);
        Assert.Equal("New description", ticket.Description);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_ShouldUpdateUpdatedAt()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title",
            "Old description");
        
        DateTime oldUpdatedAt = ticket.UpdatedAt;
        ticket.UpdateDetails("New title", "New description");
        
        Assert.NotEqual(oldUpdatedAt, ticket.UpdatedAt);
    }
    
    [Fact]
    public void ChangePriority_WhenCalled_ShouldSetNewPriority()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description",
            TicketPriority.Low);

        ticket.ChangePriority(TicketPriority.High);

        Assert.Equal(TicketPriority.High, ticket.Priority);
    }
    
    [Fact]
    public void ChangePriority_WhenCalled_ShouldUpdateUpdatedAt()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        DateTime oldUpdatedAt = ticket.UpdatedAt;

        ticket.ChangePriority(TicketPriority.High);

        Assert.NotEqual(oldUpdatedAt, ticket.UpdatedAt);
    }

    [Fact]
    public void AssignTo_WhenCalled_ShouldSetAssignedToSupportId()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");
        
        Guid supportId = Guid.NewGuid();
        
        ticket.AssignTo(supportId);
        
        Assert.Equal(supportId, ticket.AssignedToSupportId);
    }
    
    [Fact]
    public void AssignTo_WhenCalled_ShouldUpdateUpdatedAt()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        Guid supportUserId = Guid.NewGuid();
        DateTime oldUpdatedAt = ticket.UpdatedAt;

        ticket.AssignTo(supportUserId);

        Assert.NotEqual(oldUpdatedAt, ticket.UpdatedAt);
    }
    
    [Fact]
    public void Unassign_WhenCalled_ShouldSetAssignedToSupportId()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");
        
        Guid supportId = Guid.NewGuid();
        ticket.AssignTo(supportId);
        ticket.Unassign();
        
        Assert.Null(ticket.AssignedToSupportId);
    }
    
    [Fact]
    public void Unassign_WhenCalled_ShouldUpdateUpdatedAt()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        Guid supportId = Guid.NewGuid();
        ticket.AssignTo(supportId);
        DateTime oldUpdatedAt = ticket.UpdatedAt;
        ticket.Unassign();

        
        Assert.NotEqual(oldUpdatedAt, ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_WhenCalled_ShouldSetNewStatus()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");
        
        TicketStatus status = TicketStatus.InProgress;
        
        ticket.ChangeStatus(status);
        
        Assert.Equal(status, ticket.Status);
    }
    
    [Fact]
    public void ChangeStatus_WhenCalled_ShouldUpdateUpdatedAt()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        DateTime oldUpdatedAt = ticket.UpdatedAt;

        ticket.ChangeStatus(TicketStatus.InProgress);

        Assert.NotEqual(oldUpdatedAt, ticket.UpdatedAt);
    }

    [Fact]
    public void ChangeCategory_WhenCalled_ShouldChangeCategoryId()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        Guid newCategoryId = Guid.NewGuid();
        ticket.ChangeCategory(newCategoryId);

        Assert.Equal(newCategoryId, ticket.CategoryId);
    }

    [Fact]
    public void ChangeCategory_WhenCalled_ShouldUpdateUpdatedAt()
    {
        Ticket ticket = Ticket.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        DateTime oldUpdatedAt = ticket.UpdatedAt;
        ticket.ChangeCategory(Guid.NewGuid());

        Assert.NotEqual(oldUpdatedAt, ticket.UpdatedAt);
    }
}