using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ResetPasswordRequest(
    [Required(AllowEmptyStrings = false)] 
    string Token,
    
    [Required(AllowEmptyStrings = false)] 
    string NewPassword,
    
    [Required(AllowEmptyStrings = false)] 
    string ConfirmPassword);
