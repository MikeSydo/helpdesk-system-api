using HelpDesk.Application.Categories;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets;

public sealed class TicketService(
    ITicketRepository ticketRepository,
    ICategoryRepository categoryRepository,
    ICurrentUser? currentUser = null) : ITicketService
{
    public async Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is not null && !CanAccess(ticket))
        {
            return null;
        }

        return ticket == null ? null : MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Ticket> tickets = await ticketRepository.GetAllAsync(cancellationToken);
        if (currentUser?.Role == Domain.Enums.UserRole.User && currentUser.UserId is Guid userId)
        {
            tickets = tickets
                .Where(ticket => ticket.CreatedByUserId == userId)
                .ToList();
        }

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
            throw new ForbiddenException("You cannot update this ticket.");
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
        
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        
        return MapToResponse(ticket);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return false;

        if (!CanAccess(ticket))
        {
            throw new ForbiddenException("You cannot delete this ticket.");
        }
        
        await ticketRepository.DeleteAsync(id, cancellationToken); 
        
        return true;
    }

    private bool CanAccess(Ticket ticket)
    {
        if (currentUser is null || currentUser.Role is null)
        {
            return true;
        }

        return currentUser.Role is Domain.Enums.UserRole.Support or Domain.Enums.UserRole.Admin
            || currentUser.UserId == ticket.CreatedByUserId;
    }
    
    private static TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse(ticket.Id, ticket.Title, ticket.Description, ticket.Status, 
            ticket.Priority, ticket.CategoryId, ticket.CreatedByUserId, ticket.AssignedToSupportId, 
            ticket.CreatedAt, ticket.UpdatedAt);
    }
}