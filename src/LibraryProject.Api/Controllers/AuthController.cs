using LibraryProject.Application.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authenticationService.RegisterAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authenticationService.LoginAsync(request, cancellationToken);

        return Ok(response);
    }
}
