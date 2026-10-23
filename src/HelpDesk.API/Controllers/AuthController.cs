using HelpDesk.Application.Auth;
using HelpDesk.Application.Auth.Dtos;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IUserManagementService userManagementService,
    ICurrentUser currentUser) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<PendingRegistrationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        PendingRegistrationResponse response =
            await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        await authService.ConfirmEmailAsync(request, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        await authService.ResendConfirmationAsync(request, cancellationToken);
        return Ok(new { Message = "If the account exists, an email has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.RefreshAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        await authService.ChangePasswordAsync(
            currentUser.UserId.Value,
            request,
            cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(new { Message = "If the account exists, an email has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        bool deleted = await userManagementService.DeleteAccountAsync(
            currentUser.UserId.Value,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
