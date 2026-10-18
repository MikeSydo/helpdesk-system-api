using HelpDesk.Application.Categories;
using HelpDesk.Application.Categories.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetCategory(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (category == null) return NotFound(id);
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        CategoryResponse category = await _categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            category);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.UpdateAsync(id, request, cancellationToken);
        if  (category == null) return NotFound(id);
        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var status = await _categoryService.DeleteAsync(id, cancellationToken);
        return status ? Ok(status) : NotFound(id);
    }
}