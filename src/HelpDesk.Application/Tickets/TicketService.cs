using HelpDesk.Application.Categories;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets;

public sealed class TicketService(ITicketRepository ticketRepository, ICategoryRepository categoryRepository) : ITicketService
{
    public async Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        return ticket == null ? null : MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Ticket> tickets = await ticketRepository.GetAllAsync(cancellationToken);
        return [.. tickets.Select(MapToResponse)];
    }

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (request.CategoryId == Guid.Empty)
            throw new ArgumentException("CategoryId cannot be empty.", nameof(request.CategoryId));
        
        Category? category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");
        
        Ticket ticket = Ticket.Create(request.CategoryId, currentUserId, request.Title, request.Description,
            request.Priority);
        
        await ticketRepository.AddAsync(ticket, cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return null;
        
        if (ticket.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this ticket.");
        }
        
        if (ticket.CategoryId != request.CategoryId)
        {
            var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");

            ticket.ChangeCategory(request.CategoryId);
        }            
        
        ticket.UpdateDetails(request.Title, request.Description);
        ticket.ChangePriority(request.Priority);
        
        await ticketRepository.SaveChangesAsync(cancellationToken);
        
        return MapToResponse(ticket);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return false;
        
        await ticketRepository.DeleteAsync(id, cancellationToken); 
        
        return true;
    }
    
    public async Task<TicketResponse?> ChangeStatusAsync(Guid ticketId, TicketStatus newStatus, CancellationToken
        cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        ticket.ChangeStatus(newStatus);
        await ticketRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> AssignSupportAsync(Guid ticketId, Guid supportUserId, CancellationToken
        cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        ticket.AssignTo(supportUserId);
        await ticketRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UnassignSupportAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null) return null;

        ticket.Unassign();
        await ticketRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(ticket);
    }
    
    private static TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse(ticket.Id, ticket.Title, ticket.Description, ticket.Status, 
            ticket.Priority, ticket.CategoryId, ticket.CreatedByUserId, ticket.AssignedToSupportId, 
            ticket.CreatedAt, ticket.UpdatedAt);
    }
}