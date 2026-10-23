using HelpDesk.Application.Auth.Dtos;

namespace HelpDesk.Application.Auth;

public interface IAuthService
{
    Task<PendingRegistrationResponse> RegisterAsync(RegisterRequest request, 
        CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}