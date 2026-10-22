using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

public sealed record RefreshTokenRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "RefreshToken is required.")]
    string RefreshToken);
