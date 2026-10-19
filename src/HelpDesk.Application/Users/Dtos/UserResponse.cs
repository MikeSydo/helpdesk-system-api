namespace HelpDesk.Application.Users.Dtos;

public record UserResponse( 
    Guid Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive);