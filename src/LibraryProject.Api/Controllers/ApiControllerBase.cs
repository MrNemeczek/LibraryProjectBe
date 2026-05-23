using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    protected string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}
