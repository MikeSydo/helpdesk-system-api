using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Application.Auth.Dtos;

public sealed record LogoutRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "RefreshToken is required.")]
    string RefreshToken);
