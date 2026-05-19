using LibraryProject.Application.Authentication.Exceptions;
using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Authentication;

internal sealed class AuthenticationService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IUserPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthenticationService
{
    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var emailExists = await userRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailExists)
        {
            throw new EmailAlreadyExistsException(email);
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = UserRole.Reader,
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthenticationResponse(user);
    }

    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var isPasswordValid = passwordHasher.VerifyPassword(user, request.Password);
        if (!isPasswordValid)
        {
            throw new InvalidCredentialsException();
        }

        return CreateAuthenticationResponse(user);
    }

    private AuthenticationResponse CreateAuthenticationResponse(User user)
    {
        var token = jwtTokenGenerator.GenerateToken(user);
        return new AuthenticationResponse(
            token.Token,
            token.ExpiresAt,
            new AuthenticatedUserResponse(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString()));
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
