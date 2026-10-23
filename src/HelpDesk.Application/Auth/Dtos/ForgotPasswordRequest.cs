using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record ForgotPasswordRequest(
    [Required(AllowEmptyStrings = false)] 
    string Email);
