using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CategoryId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToSupportId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Category Category { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public User? AssignedToSupport { get; private set; }
    public List<TicketComment> Comments { get; private set; } = [];

    private Ticket(Guid categoryId, Guid createdByUserId, string title, string description, TicketStatus status,
        TicketPriority priority)
    {
        CategoryId = categoryId;
        CreatedByUserId = createdByUserId;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
    }

    public static Ticket Create(Guid categoryId, Guid createdByUserId, string title, string description, 
        TicketPriority priority = TicketPriority.Medium)
    {
        if (categoryId == Guid.Empty) throw new ArgumentException("Category is required.", nameof(categoryId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("Author is required.", nameof(createdByUserId));
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Ticket(categoryId, createdByUserId, title.Trim(), description.Trim(), TicketStatus.Open, priority);
    }
    public void UpdateDetails(string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Title = title.Trim();
        Description = description.Trim();
        Touch();
    }

    public void ChangeCategory(Guid categoryId)
    {
        if(categoryId == Guid.Empty ) 
            throw new ArgumentException("Category is required.", nameof(categoryId));
        CategoryId = categoryId;
        Touch();
    }
    
    public void ChangeStatus(TicketStatus newStatus)
    {
        if (newStatus == Status)
            return;

        bool isAllowed = (Status, newStatus) switch
        {
            (TicketStatus.Open, TicketStatus.InProgress) => true,
            (TicketStatus.Open, TicketStatus.Canceled) => true,
            (TicketStatus.InProgress, TicketStatus.Resolved) => true,
            (TicketStatus.InProgress, TicketStatus.Canceled) => true,
            (TicketStatus.Resolved, TicketStatus.Closed) => true,
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Ticket status cannot change from {Status} to {newStatus}.");
        }

        Status = newStatus;
        Touch();
    }
    public void ChangePriority(TicketPriority priority)
    {
        Priority = priority;
        Touch();
    }
    public void AssignTo(Guid supportUserId)
    {
        if (supportUserId == Guid.Empty) throw new ArgumentException("Support user is required.");
        AssignedToSupportId = supportUserId;
        Touch();
    }
    public void Unassign()
    {
        AssignedToSupportId = null;
        Touch();
    }
    
    private void Touch() => UpdatedAt = DateTime.UtcNow;
}