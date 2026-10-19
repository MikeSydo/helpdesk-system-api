using HelpDesk.Application.Users.Dtos;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Users;

public sealed class UserManagementService(IUserRepository userRepository) : IUserManagementService
{
    public async Task<UserResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = await userRepository.GetAllUsers(cancellationToken);
        return [.. users.Select(MapToResponse)];
    }

    public async Task<UserResponse?> ChangeRoleAsync(
        Guid id,
        ChangeRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        User? user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.ChangeRole(request.Role);
        await userRepository.UpdateAsync(user, cancellationToken);

        return MapToResponse(user);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.Deactivate();
        await userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.CreatedAt,
            user.UpdatedAt,
            user.IsActive);
    }
}
