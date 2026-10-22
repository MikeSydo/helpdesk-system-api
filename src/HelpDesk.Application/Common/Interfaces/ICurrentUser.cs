using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
