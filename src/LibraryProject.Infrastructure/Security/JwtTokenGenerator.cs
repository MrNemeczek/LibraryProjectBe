using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryProject.Application.Authentication;
using LibraryProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LibraryProject.Infrastructure.Security;

internal sealed class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public JwtToken GenerateToken(User user)
    {
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret is missing. Set 'Jwt:Secret' in appsettings.Local.json.");
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is missing. Set 'Jwt:Issuer' in configuration.");
        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is missing. Set 'Jwt:Audience' in configuration.");
        var expiresAt = DateTime.UtcNow.AddMinutes(GetExpiresMinutes());

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private int GetExpiresMinutes()
    {
        return int.TryParse(configuration["Jwt:ExpiresMinutes"], out var expiresMinutes) && expiresMinutes > 0
            ? expiresMinutes
            : 120;
    }
}
