using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public sealed record ChangeTicketStatusRequest(TicketStatus Status);
