using HelpDesk.Application.Tickets.Dtos;

namespace HelpDesk.Application.Tickets;

public interface ITicketService
{
    public Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<TicketPageResponse> GetPageAsync(
        TicketQuery query,
        CancellationToken cancellationToken = default);
    public Task<TicketResponse> CreateAsync(CreateTicketRequest request, Guid currentUserId,
        CancellationToken cancellationToken = default);
    public Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, Guid currentUserId,
        CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}