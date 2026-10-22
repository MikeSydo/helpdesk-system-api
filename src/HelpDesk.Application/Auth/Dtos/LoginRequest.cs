using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

public record LoginRequest(
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Email is required.")]
    string Email,
    
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Password is required.")]
    string Password);