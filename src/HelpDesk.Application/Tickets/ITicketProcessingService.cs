using HelpDesk.Application.Tickets.Dtos;

namespace HelpDesk.Application.Tickets;

public interface ITicketProcessingService
{
    Task<TicketResponse?> ChangeStatusAsync(
        Guid ticketId,
        ChangeTicketStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<TicketResponse?> AssignSupportAsync(
        Guid ticketId,
        AssignTicketRequest request,
        CancellationToken cancellationToken = default);

    Task<TicketResponse?> UnassignSupportAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}