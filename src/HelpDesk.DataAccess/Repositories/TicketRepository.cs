using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.DataAccess.Repositories;

public class TicketRepository(AppDbContext dbContext) : ITicketRepository
{
    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tickets.AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(
        TicketQuery query,
        Guid? ownerId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Ticket> tickets = dbContext.Tickets.AsNoTracking();

        if (ownerId is Guid userId)
        {
            tickets = tickets.Where(ticket => ticket.CreatedByUserId == userId);
        }

        if (query.Status is TicketStatus status)
        {
            tickets = tickets.Where(ticket => ticket.Status == status);
        }

        if (query.Priority is TicketPriority priority)
        {
            tickets = tickets.Where(ticket => ticket.Priority == priority);
        }

        if (query.CategoryId is Guid categoryId)
        {
            tickets = tickets.Where(ticket => ticket.CategoryId == categoryId);
        }

        int totalCount = await tickets.CountAsync(cancellationToken);
        List<Ticket> items = await tickets
            .OrderBy(ticket => ticket.CreatedAt)
            .ThenBy(ticket => ticket.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(ticket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await dbContext.Tickets.Where(t => t.Id == ticket.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Title, ticket.Title)
                .SetProperty(t => t.Description, ticket.Description)
                .SetProperty(t => t.CategoryId, ticket.CategoryId)
                .SetProperty(t => t.AssignedToSupportId, ticket.AssignedToSupportId)
                .SetProperty(t => t.Priority, ticket.Priority)
                .SetProperty(t => t.Status, ticket.Status)
                .SetProperty(t => t.UpdatedAt, ticket.UpdatedAt)
                ,cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await dbContext.Tickets.Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}