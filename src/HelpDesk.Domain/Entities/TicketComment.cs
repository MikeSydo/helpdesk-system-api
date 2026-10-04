namespace HelpDesk.Domain.Entities;

public class TicketComment
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Ticket Ticket { get; private set; } = null!;
    public User AuthorUser { get; private set; } = null!;

    private TicketComment(Guid ticketId, Guid authorUserId, string content)
    {
        TicketId = ticketId;
        AuthorUserId = authorUserId;
        Content = content;
    }

    public static TicketComment Create(Guid ticketId, Guid authorUserId, string content)
    {
        if (ticketId == Guid.Empty) throw new ArgumentException("Ticket is required.", nameof(ticketId));
        if (authorUserId == Guid.Empty) throw new ArgumentException("Author is required.", nameof(authorUserId));
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new TicketComment(ticketId, authorUserId, content.Trim());
    }
}