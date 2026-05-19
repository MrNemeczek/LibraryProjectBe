namespace LibraryProject.Application.Authentication;

public sealed record JwtToken(string Token, DateTime ExpiresAt);
