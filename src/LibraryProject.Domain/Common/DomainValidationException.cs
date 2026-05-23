namespace LibraryProject.Domain.Common;

public sealed class DomainValidationException(
    string code,
    string message,
    string fieldName,
    string error) : DomainException(
        code,
        message,
        new Dictionary<string, string[]>
        {
            [fieldName] = [error]
        });
