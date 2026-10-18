namespace HelpDesk.Application.Categories.Dtos;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description);