using HelpDesk.Application.Users;
using HelpDesk.Application.Users.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.API.Security;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<UserResponse> users =
            await userManagementService.GetAllAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        UserResponse? user = await userManagementService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound(id) : Ok(user);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<UserResponse>> ChangeRole(
        Guid id,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken)
    {
        UserResponse? user =
            await userManagementService.ChangeRoleAsync(id, request, cancellationToken);

        return user is null ? NotFound(id) : Ok(user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        bool deactivated = await userManagementService.DeactivateAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound(id);
    }
}
