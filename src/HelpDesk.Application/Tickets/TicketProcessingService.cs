using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Tickets;

public sealed class TicketProcessingService(
    ITicketRepository ticketRepository,
    IUserRepository userRepository) : ITicketProcessingService
{
    public async Task<TicketResponse?> ChangeStatusAsync(
        Guid ticketId,
        ChangeTicketStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        ticket.ChangeStatus(request.Status);
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> AssignSupportAsync(
        Guid ticketId,
        AssignTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        User? supportUser = await userRepository.GetByIdAsync(
            request.SupportUserId,
            cancellationToken);
        if (supportUser == null)
        {
            throw new KeyNotFoundException(
                $"Support user with ID {request.SupportUserId} was not found.");
        }

        // TODO: Require Support/Admin caller authorization and validate target role/activity.
        ticket.AssignTo(supportUser.Id);
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UnassignSupportAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        ticket.Unassign();
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    private static TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CategoryId,
            ticket.CreatedByUserId,
            ticket.AssignedToSupportId,
            ticket.CreatedAt,
            ticket.UpdatedAt);
    }
}