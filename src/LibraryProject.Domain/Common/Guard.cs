namespace LibraryProject.Domain.Common;

public static class Guard
{
    public static string Required(string? value, string fieldName, int maxLength, string? prefix = null)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedValue))
            throw RequiredError(fieldName, prefix);
        if (normalizedValue.Length > maxLength)
            throw TooLongError(fieldName, maxLength, prefix);
        return normalizedValue;
    }

    public static string Optional(string? value, string fieldName, int maxLength, string? prefix = null)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (normalizedValue.Length > maxLength)
            throw TooLongError(fieldName, maxLength, prefix);
        return normalizedValue;
    }

    public static DomainValidationException RequiredError(string fieldName, string? prefix = null)
    {
        var code = BuildCode(fieldName, "REQUIRED", prefix);
        return new DomainValidationException(code, $"{fieldName} is required.", fieldName, $"{fieldName} is required.");
    }

    public static DomainValidationException TooLongError(string fieldName, int maxLength, string? prefix = null)
    {
        var code = BuildCode(fieldName, "TOO_LONG", prefix);
        return new DomainValidationException(
            code,
            $"{fieldName} is too long.",
            fieldName,
            $"{fieldName} cannot exceed {maxLength} characters.");
    }

    private static string BuildCode(string fieldName, string suffix, string? prefix)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(prefix))
            parts.Add(prefix.ToUpperInvariant());
        parts.Add(fieldName.ToUpperInvariant());
        parts.Add(suffix);
        return string.Join("_", parts);
    }
}
