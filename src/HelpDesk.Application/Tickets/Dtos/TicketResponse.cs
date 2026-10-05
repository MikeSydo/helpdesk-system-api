using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public record TicketResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid CategoryId,
    Guid CreatedByUserId,
    Guid? AssignedToSupportId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);