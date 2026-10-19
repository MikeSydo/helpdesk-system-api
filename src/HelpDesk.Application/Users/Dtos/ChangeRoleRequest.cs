using System.ComponentModel.DataAnnotations;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Users.Dtos;

public record ChangeRoleRequest(
    [Required]
    UserRole Role);