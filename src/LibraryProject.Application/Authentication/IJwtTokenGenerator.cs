using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Authentication;

public interface IJwtTokenGenerator
{
    JwtToken GenerateToken(User user);
}
