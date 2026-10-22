using HelpDesk.Application.Auth.Dtos;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Auth;

public sealed class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, 
    IJwtTokenService jwtTokenService) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string TemporaryPasswordHash = "password-hash-pending";
    private const int MinimumPasswordLength = 8;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePassword(request.Password, request.ConfirmPassword);

        User user = User.Create(request.Name, request.Email, TemporaryPasswordHash);
        User? existingUser = await userRepository.GetUserByEmailAsync(user.Email, cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        user.UpdatePasswordHash(passwordHasher.Hash(user, request.Password));
        await userRepository.AddAsync(user, cancellationToken);

        AuthTokenResult tokens = await jwtTokenService.IssueAsync(user, cancellationToken);

        return MapResponse(tokens);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

        User? user = await userRepository.GetUserByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(user, request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        AuthTokenResult tokens = await jwtTokenService.IssueAsync(user, cancellationToken);

        return MapResponse(tokens);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);

        AuthTokenResult tokens = await jwtTokenService.RefreshAsync(request.RefreshToken, cancellationToken);

        return MapResponse(tokens);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);

        await jwtTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
    }

    private static void ValidatePassword(string password, string confirmPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmPassword);

        if (password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException(
                $"Password must be at least {MinimumPasswordLength} characters long.",
                nameof(password));
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Password and confirmation password must match.",
                nameof(confirmPassword));
        }
    }

    private static AuthResponse MapResponse(AuthTokenResult tokens) =>
        new(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.User.Id,
            tokens.User.Name,
            tokens.User.Email,
            tokens.User.Role.ToString());
}