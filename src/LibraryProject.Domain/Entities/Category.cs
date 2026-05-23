using LibraryProject.Domain.Common;

namespace LibraryProject.Domain.Entities;

public sealed class Category
{
    public const int MaxNameLength = 100;

    private Category()
    {
    }

    private Category(string? name)
    {
        Name = NormalizeName(name);
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<Book> Books { get; private set; } = new List<Book>();

    public static Category Create(string? name)
    {
        return new Category(name);
    }

    public static string NormalizeName(string? name)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new DomainValidationException(
                "CATEGORY_NAME_REQUIRED",
                "Category name is required.",
                nameof(Name),
                "Category name is required.");
        }

        if (normalizedName.Length > MaxNameLength)
        {
            throw new DomainValidationException(
                "CATEGORY_NAME_TOO_LONG",
                "Category name is too long.",
                nameof(Name),
                $"Category name cannot exceed {MaxNameLength} characters.");
        }

        return normalizedName;
    }
}
