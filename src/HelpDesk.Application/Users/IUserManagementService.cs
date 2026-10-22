using HelpDesk.Application.Users.Dtos;

namespace HelpDesk.Application.Users;

public interface IUserManagementService
{
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> ChangeRoleAsync(Guid id, ChangeRoleRequest request,
        CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default);
}