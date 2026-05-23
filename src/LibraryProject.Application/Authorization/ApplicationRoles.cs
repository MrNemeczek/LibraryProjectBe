using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Authorization;

public static class ApplicationRoles
{
    public const string Reader = nameof(UserRole.Reader);
    public const string Librarian = nameof(UserRole.Librarian);
    public const string Administrator = nameof(UserRole.Administrator);
    public const string LibrarianOrAdministrator = Librarian + "," + Administrator;
}
