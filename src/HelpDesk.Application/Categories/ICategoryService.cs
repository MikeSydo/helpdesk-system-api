using HelpDesk.Application.Categories.Dtos;

namespace HelpDesk.Application.Categories;

public interface ICategoryService
{
    public Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<CategoryResponse> CreateAsync(CreateCategoryRequest request,
        CancellationToken cancellationToken = default);
    public Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}