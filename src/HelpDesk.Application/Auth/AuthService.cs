using System.Security.Cryptography;
using System.Text;
using HelpDesk.Application.Auth.Dtos;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOneTimeTokenRepository oneTimeTokenRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IEmailSender emailSender,
    EmailOptions emailOptions) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string TemporaryPasswordHash = "password-hash-pending";
    private const int MinimumPasswordLength = 8;

    public async Task<PendingRegistrationResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePassword(request.Password, request.ConfirmPassword);

        User user = User.Create(request.Name, request.Email, TemporaryPasswordHash);
        User? existingUser = await userRepository.GetUserByEmailAsync(
            user.Email,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        user.UpdatePasswordHash(passwordHasher.Hash(user, request.Password));
        string rawToken = CreateRawToken();
        OneTimeToken confirmationToken = OneTimeToken.Create(
            user.Id,
            OneTimeTokenType.EmailConfirmation,
            HashToken(rawToken),
            DateTime.UtcNow.AddHours(24));

        await emailSender.SendAsync(
            user.Email,
            "Confirm your HelpDesk account",
            BuildLinkMessage(
                "Please confirm your email address using this link:",
                emailOptions.ConfirmationLinkBaseUrl,
                rawToken),
            cancellationToken);

        await userRepository.AddAsync(user, cancellationToken);
        await oneTimeTokenRepository.AddAsync(confirmationToken, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new PendingRegistrationResponse(
            user.Id,
            user.Email,
            "Registration completed. Check your email to confirm your account.");
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);

        User? user = await userRepository.GetUserByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !user.EmailConfirmed ||
            !passwordHasher.Verify(user, request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        AuthTokenResult tokens = await jwtTokenService.IssueAsync(user, cancellationToken);
        return MapResponse(tokens);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);
        AuthTokenResult tokens = await jwtTokenService.RefreshAsync(
            request.RefreshToken,
            cancellationToken);
        return MapResponse(tokens);
    }

    public async Task LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RefreshToken);
        await jwtTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
    }

    public async Task ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Token);

        OneTimeToken token = await GetTokenAsync(
            request.Token,
            OneTimeTokenType.EmailConfirmation,
            cancellationToken);

        User? user = await userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid or expired token.");
        }

        user.ConfirmEmail();
        token.MarkUsed();
        await userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ResendConfirmationAsync(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);

        User? user = await userRepository.GetUserByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null || !user.IsActive || user.EmailConfirmed)
        {
            return;
        }

        await SendNewTokenAsync(
            user,
            OneTimeTokenType.EmailConfirmation,
            emailOptions.ConfirmationLinkBaseUrl,
            "Confirm your HelpDesk account",
            "Please confirm your email address using this link:",
            cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        ArgumentNullException.ThrowIfNull(request);
        ValidatePassword(request.NewPassword, request.ConfirmPassword);

        User? user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive || !user.EmailConfirmed)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        if (!passwordHasher.Verify(user, request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is invalid.");
        }

        user.UpdatePasswordHash(passwordHasher.Hash(user, request.NewPassword));
        await refreshTokenRepository.RevokeForUserAsync(
            user.Id,
            "password_changed",
            cancellationToken);
        await userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);

        User? user = await userRepository.GetUserByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null || !user.IsActive || !user.EmailConfirmed)
        {
            return;
        }

        await SendNewTokenAsync(
            user,
            OneTimeTokenType.PasswordReset,
            emailOptions.PasswordResetLinkBaseUrl,
            "Reset your HelpDesk password",
            "Reset your password using this link:",
            cancellationToken);
    }

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Token);
        ValidatePassword(request.NewPassword, request.ConfirmPassword);

        OneTimeToken token = await GetTokenAsync(
            request.Token,
            OneTimeTokenType.PasswordReset,
            cancellationToken);

        User? user = await userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null || !user.IsActive || !user.EmailConfirmed)
        {
            throw new UnauthorizedException("Invalid or expired token.");
        }

        user.UpdatePasswordHash(
            passwordHasher.Hash(user, request.NewPassword));
        token.MarkUsed();
        await refreshTokenRepository.RevokeForUserAsync(
            token.UserId,
            "password_reset",
            cancellationToken);
        await userRepository.UpdateAsync(user, cancellationToken);
    }

    private async Task<OneTimeToken> GetTokenAsync(
        string rawToken,
        OneTimeTokenType type,
        CancellationToken cancellationToken)
    {
        OneTimeToken? token = await oneTimeTokenRepository.GetActiveAsync(
            HashToken(rawToken),
            type,
            cancellationToken);

        if (token is null)
        {
            throw new UnauthorizedException("Invalid or expired token.");
        }

        return token;
    }

    private async Task SendNewTokenAsync(
        User user,
        OneTimeTokenType type,
        string linkBaseUrl,
        string subject,
        string prefix,
        CancellationToken cancellationToken)
    {
        await oneTimeTokenRepository.InvalidateActiveAsync(
            user.Id,
            type,
            cancellationToken);

        string rawToken = CreateRawToken();
        OneTimeToken token = OneTimeToken.Create(
            user.Id,
            type,
            HashToken(rawToken),
            type == OneTimeTokenType.EmailConfirmation
                ? DateTime.UtcNow.AddHours(24)
                : DateTime.UtcNow.AddHours(1));

        await emailSender.SendAsync(
            user.Email,
            subject,
            BuildLinkMessage(prefix, linkBaseUrl, rawToken),
            cancellationToken);

        await oneTimeTokenRepository.AddAsync(token, cancellationToken);
        await oneTimeTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private static string BuildLinkMessage(
        string prefix,
        string baseUrl,
        string rawToken) =>
        $"{prefix}{Environment.NewLine}{baseUrl.TrimEnd('/')}" +
        $"?token={Uri.EscapeDataString(rawToken)}";

    private static string CreateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    private static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

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
