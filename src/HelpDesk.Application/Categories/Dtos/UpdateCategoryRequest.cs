namespace HelpDesk.Application.Categories.Dtos;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description);