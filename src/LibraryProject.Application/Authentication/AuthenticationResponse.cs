namespace LibraryProject.Application.Authentication;

public sealed record AuthenticationResponse(
    string Token,
    DateTime ExpiresAt,
    AuthenticatedUserResponse User);
