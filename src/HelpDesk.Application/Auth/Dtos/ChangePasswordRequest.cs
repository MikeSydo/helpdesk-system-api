using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ChangePasswordRequest(
    [Required(AllowEmptyStrings = false)] 
    string CurrentPassword,
    
    [Required(AllowEmptyStrings = false)] 
    string NewPassword,
    
    [Required(AllowEmptyStrings = false)] 
    string ConfirmPassword);
