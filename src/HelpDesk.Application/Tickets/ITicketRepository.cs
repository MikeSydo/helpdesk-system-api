using HelpDesk.Domain.Entities;
using HelpDesk.Application.Tickets.Dtos;

namespace HelpDesk.Application.Tickets;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(TicketQuery query, Guid? ownerId,
        CancellationToken cancellationToken = default);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}