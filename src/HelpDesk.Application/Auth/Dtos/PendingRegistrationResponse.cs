namespace HelpDesk.Application.Auth.Dtos;

public sealed record PendingRegistrationResponse(
    Guid UserId,
    string Email,
    string Message);
