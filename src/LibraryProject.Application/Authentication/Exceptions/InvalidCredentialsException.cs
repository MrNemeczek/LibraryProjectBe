using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Authentication.Exceptions;

public sealed class InvalidCredentialsException() : ApplicationExceptionBase(
    "AUTH_INVALID_CREDENTIALS",
    "Invalid email or password.",
    (int)HttpStatusCode.Unauthorized);
