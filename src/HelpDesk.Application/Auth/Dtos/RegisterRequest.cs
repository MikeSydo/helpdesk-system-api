using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

public record RegisterRequest(
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Name is required.")]
    string Name,
    
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Email is required.")]
    string Email,
    
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Password is required.")]
    string Password,
    
    [Required(AllowEmptyStrings = false,  ErrorMessage = "ConfirmPassword is required.")]
    string ConfirmPassword);