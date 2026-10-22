using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets;

public sealed class TicketProcessingService(
    ITicketRepository ticketRepository,
    IUserRepository userRepository,
    ICurrentUser? currentUser = null) : ITicketProcessingService
{
    public async Task<TicketResponse?> ChangeStatusAsync(
        Guid ticketId,
        ChangeTicketStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureProcessingCaller();

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
        EnsureProcessingCaller();

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

        if (supportUser.Role != UserRole.Support)
        {
            throw new ForbiddenException(
                "Only support users can be assigned to tickets.");
        }

        if (!supportUser.IsActive)
        {
            throw new ForbiddenException(
                "An inactive support user cannot be assigned to a ticket.");
        }

        ticket.AssignTo(supportUser.Id);
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UnassignSupportAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        EnsureProcessingCaller();

        Ticket? ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        ticket.Unassign();
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    private void EnsureProcessingCaller()
    {
        if (currentUser is null)
        {
            return;
        }

        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        if (currentUser.Role is not (UserRole.Support or UserRole.Admin))
        {
            throw new ForbiddenException(
                "Only support or admin users can process tickets.");
        }
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