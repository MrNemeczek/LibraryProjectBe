using System.ComponentModel.DataAnnotations;

namespace LibraryProject.Application.Authentication;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, MaxLength(100)] string Password);
