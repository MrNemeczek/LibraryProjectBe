using System.ComponentModel.DataAnnotations;

namespace LibraryProject.Application.Authentication;

public sealed record RegisterRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);
