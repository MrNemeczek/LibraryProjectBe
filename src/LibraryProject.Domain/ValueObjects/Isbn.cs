using LibraryProject.Domain.Common;

namespace LibraryProject.Domain.ValueObjects;

public sealed record Isbn
{
    public const int MaxLength = 20;
    private const string FieldName = "Isbn";

    private Isbn(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Isbn Create(string? value)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new DomainValidationException(
                "BOOK_ISBN_REQUIRED",
                "ISBN is required.",
                FieldName,
                "ISBN is required.");
        }

        if (normalizedValue.Length > MaxLength)
        {
            throw new DomainValidationException(
                "BOOK_ISBN_TOO_LONG",
                "ISBN is too long.",
                FieldName,
                $"ISBN cannot exceed {MaxLength} characters.");
        }

        return new Isbn(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }
}
