using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Authentication.Exceptions;

public sealed class EmailAlreadyExistsException(string email) : ApplicationExceptionBase(
    "AUTH_EMAIL_ALREADY_EXISTS",
    "User with this email already exists.",
    (int)HttpStatusCode.Conflict,
    new Dictionary<string, string[]>
    {
        [nameof(email)] = ["Email is already registered."]
    })
{
    public string Email { get; } = email;
}
