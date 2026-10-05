using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public sealed record UpdateTicketRequest(
    string Title,
    string Description,
    Guid CategoryId,
    TicketPriority Priority);