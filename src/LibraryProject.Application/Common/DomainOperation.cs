using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Domain.Common;

namespace LibraryProject.Application.Common;

public static class DomainOperation
{
    public static T Execute<T>(Func<T> operation)
    {
        try { return operation(); }
        catch (DomainException exception) { throw new DomainRuleViolationException(exception); }
    }

    public static void Execute(Action operation)
    {
        try { operation(); }
        catch (DomainException exception) { throw new DomainRuleViolationException(exception); }
    }
}
