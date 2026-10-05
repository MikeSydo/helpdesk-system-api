using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Tickets;

public sealed class TicketService(ITicketRepository ticketRepository) : ITicketService
{
    private readonly ITicketRepository _ticketRepository = ticketRepository;

    public async Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        return ticket == null ? null : MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Ticket> tickets = await _ticketRepository.GetAllAsync(cancellationToken);
        return [.. tickets.Select(MapToResponse)];
    }

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        Ticket ticket = Ticket.Create(request.CategoryId, currentUserId, request.Title, request.Description,
            request.Priority);
        
        await _ticketRepository.AddAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return null;
        
        ticket.UpdateDetails(request.Title, request.Description);
        ticket.ChangePriority(request.Priority);
        
        await _ticketRepository.SaveChangesAsync(cancellationToken);
        
        return MapToResponse(ticket);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return false;
        
        await _ticketRepository.DeleteAsync(id, cancellationToken); 
        
        return true;
    }

    private static TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse(ticket.Id, ticket.Title, ticket.Description, ticket.Status, 
            ticket.Priority, ticket.CategoryId, ticket.CreatedByUserId, ticket.AssignedToSupportId, 
            ticket.CreatedAt, ticket.UpdatedAt);
    }
}