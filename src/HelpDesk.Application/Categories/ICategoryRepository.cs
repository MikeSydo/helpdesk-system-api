using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id,  CancellationToken cancellationToken = default);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string requestName, CancellationToken cancellationToken);
}