using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Enums;

namespace HelpDesk.API.Security;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            string? value = Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out Guid userId) && userId != Guid.Empty ? userId : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            string? value = Principal.FindFirstValue(ClaimTypes.Role)
                ?? Principal.FindFirstValue("role");

            return Enum.TryParse(value, ignoreCase: true, out UserRole role) ? role : null;
        }
    }
}
