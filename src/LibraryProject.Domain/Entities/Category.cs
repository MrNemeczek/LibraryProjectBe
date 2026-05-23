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
        return Guard.Required(name, nameof(Name), MaxNameLength, "CATEGORY");
    }
}
