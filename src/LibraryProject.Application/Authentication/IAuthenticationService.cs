namespace LibraryProject.Application.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
