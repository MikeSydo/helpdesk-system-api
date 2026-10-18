using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets;

public interface ITicketService
{
    public Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid currentUserId,
        CancellationToken cancellationToken = default);
    public Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, Guid currentUserId,
        CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    public Task<TicketResponse?> ChangeStatusAsync(Guid ticketId, TicketStatus newStatus, 
        CancellationToken cancellationToken);
    public Task<TicketResponse?> AssignSupportAsync(Guid ticketId, Guid supportUserId,
        CancellationToken cancellationToken);
    public Task<TicketResponse?> UnassignSupportAsync(Guid ticketId, CancellationToken cancellationToken);

}