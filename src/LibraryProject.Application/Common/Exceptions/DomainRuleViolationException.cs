using System.Net;
using LibraryProject.Domain.Common;

namespace LibraryProject.Application.Common.Exceptions;

public sealed class DomainRuleViolationException(DomainException exception) : ApplicationExceptionBase(
    exception.Code,
    exception.Message,
    (int)HttpStatusCode.BadRequest,
    exception.Errors);
