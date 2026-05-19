namespace LibraryProject.Application.Authentication;

public sealed record AuthenticatedUserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role);
