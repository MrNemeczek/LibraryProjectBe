using LibraryProject.Domain.Common;

namespace LibraryProject.Domain.ValueObjects;

public sealed record Isbn
{
    public const int MaxLength = 20;

    private Isbn(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Isbn Create(string? value)
    {
        var normalizedValue = Guard.Required(value, nameof(Value), MaxLength, "BOOK");
        return new Isbn(normalizedValue);
    }

    public override string ToString() => Value;
}
