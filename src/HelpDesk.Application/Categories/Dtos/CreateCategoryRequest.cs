using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Categories.Dtos;

public sealed record CreateCategoryRequest(
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    string Name,
    
    [MaxLength(500)]
    string? Description);