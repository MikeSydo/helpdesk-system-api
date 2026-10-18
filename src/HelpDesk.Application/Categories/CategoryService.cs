using HelpDesk.Application.Categories.Dtos;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Categories;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Category? category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        return category == null ? null : MapToResponse(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Category> categories = await categoryRepository.GetAllAsync(cancellationToken);
        return [.. categories.Select(MapToResponse)];
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, 
        CancellationToken cancellationToken = default)
    {
        Category category = Category.Create(request.Name, request.Description);
        await categoryRepository.AddAsync(category, cancellationToken);
        return MapToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request, 
        CancellationToken cancellationToken = default)
    {
        Category? category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) return null;
        
        category.Update(request.Name,  request.Description);
        
        await categoryRepository.SaveChangesAsync(cancellationToken);
            
        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Category? category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) return false;
        
        await categoryRepository.DeleteAsync(id, cancellationToken); 
        
        return true;
    }
    
    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse(category.Id, category.Name, category.Description , category.Tickets);
    }
}