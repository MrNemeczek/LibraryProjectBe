using LibraryProject.Application.Authentication;
using LibraryProject.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LibraryProject.Infrastructure.Security;

internal sealed class AspNetUserPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result != PasswordVerificationResult.Failed;
    }
}
