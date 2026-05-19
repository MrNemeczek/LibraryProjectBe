using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Authentication;

public interface IUserPasswordHasher
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}
