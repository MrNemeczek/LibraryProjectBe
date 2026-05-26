using LibraryProject.Domain.Common;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class BookCopy
{
    public const int MaxInventoryNumberLength = 50;

    private BookCopy()
    {
    }

    private BookCopy(string inventoryNumber)
    {
        InventoryNumber = Guard.Required(inventoryNumber, nameof(InventoryNumber), MaxInventoryNumberLength, "BOOKCOPY");
    }

    public int Id { get; private set; }
    public int BookId { get; private set; }
    public string InventoryNumber { get; private set; } = string.Empty;
    public BookCopyStatus Status { get; private set; } = BookCopyStatus.Available;

    public Book Book { get; private set; } = null!;
    public ICollection<Loan> Loans { get; private set; } = new List<Loan>();

    public static BookCopy Create(string inventoryNumber)
    {
        return new BookCopy(inventoryNumber);
    }

    public void Reserve()
    {
        if (Status != BookCopyStatus.Available)
            throw new DomainValidationException(
                "BOOKCOPY_NOT_AVAILABLE",
                "Book copy is not available.",
                nameof(Status),
                $"Cannot reserve a copy with status '{Status}'.");
        
        Status = BookCopyStatus.Reserved;
    }

    public void Borrow()
    {
        if (Status != BookCopyStatus.Available)
            throw new DomainValidationException(
                "BOOKCOPY_NOT_AVAILABLE",
                "Book copy is not available.",
                nameof(Status),
                $"Cannot borrow a copy with status '{Status}'.");

        Status = BookCopyStatus.Borrowed;
    }

    public void MakeAvailable()
    {
        if (Status != BookCopyStatus.Borrowed && Status != BookCopyStatus.Reserved)
            throw new DomainValidationException(
                "BOOKCOPY_NOT_BORROWED",
                "Book copy is not currently borrowed.",
                nameof(Status),
                $"Cannot make available a copy with status '{Status}'.");

        Status = BookCopyStatus.Available;
    }
}
