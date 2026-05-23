using LibraryProject.Domain.Common;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Domain.Entities;

public sealed class Book
{
    public const int MaxTitleLength = 200;
    public const int MaxAuthorLength = 200;
    public const int MaxDescriptionLength = 2000;

    private Book()
    {
    }

    private Book(string? title, string? author, Isbn isbn, string? description, Category category)
    {
        ApplyDetails(title, author, isbn, description, category);
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public Isbn Isbn { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public int CategoryId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public Category Category { get; private set; } = null!;
    public ICollection<BookCopy> Copies { get; private set; } = new List<BookCopy>();
    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();

    public static Book Create(string? title, string? author, Isbn isbn, string? description, Category category)
    {
        return new Book(title, author, isbn, description, category);
    }

    public void UpdateDetails(string? title, string? author, Isbn isbn, string? description, Category category)
    {
        ApplyDetails(title, author, isbn, description, category);
    }

    public void Delete(DateTime deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAt;
    }

    private void ApplyDetails(string? title, string? author, Isbn isbn, string? description, Category category)
    {
        Title = NormalizeRequired(title, nameof(Title), MaxTitleLength);
        Author = NormalizeRequired(author, nameof(Author), MaxAuthorLength);
        Isbn = isbn ?? throw Required(nameof(Isbn));
        Description = NormalizeOptional(description, nameof(Description), MaxDescriptionLength);
        Category = category ?? throw Required(nameof(Category));
    }

    private static string NormalizeRequired(string? value, string fieldName, int maxLength)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw Required(fieldName);
        }

        if (normalizedValue.Length > maxLength)
        {
            throw TooLong(fieldName, maxLength);
        }

        return normalizedValue;
    }

    private static string NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (normalizedValue.Length > maxLength)
        {
            throw TooLong(fieldName, maxLength);
        }

        return normalizedValue;
    }

    private static DomainValidationException Required(string fieldName)
    {
        return new DomainValidationException(
            $"BOOK_{fieldName.ToUpperInvariant()}_REQUIRED",
            $"{fieldName} is required.",
            fieldName,
            $"{fieldName} is required.");
    }

    private static DomainValidationException TooLong(string fieldName, int maxLength)
    {
        return new DomainValidationException(
            $"BOOK_{fieldName.ToUpperInvariant()}_TOO_LONG",
            $"{fieldName} is too long.",
            fieldName,
            $"{fieldName} cannot exceed {maxLength} characters.");
    }
}
