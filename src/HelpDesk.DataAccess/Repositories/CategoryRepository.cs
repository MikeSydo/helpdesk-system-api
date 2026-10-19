using HelpDesk.Application.Categories;
using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.DataAccess.Repositories;

public class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories.AsNoTracking().ToListAsync(cancellationToken : cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await dbContext.Categories.AddAsync(category,  cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        await dbContext.Categories.Where(c => c.Id == category.Id)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Name, category.Name)
                    .SetProperty(c => c.Description, category.Description)
                ,cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await dbContext.Categories.Where(c => c.Id == id)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string requestName,
        CancellationToken cancellationToken,
        Guid? excludedCategoryId = null)
    {
        string trimmedName = requestName.Trim();

        return await dbContext.Categories
            .AnyAsync(
                c => EF.Functions.ILike(c.Name, trimmedName) &&
                     (!excludedCategoryId.HasValue || c.Id != excludedCategoryId.Value),
                cancellationToken);
    }

    public Task<bool> HasTicketsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return dbContext.Tickets.AnyAsync(t => t.CategoryId == categoryId, cancellationToken);
    }
}