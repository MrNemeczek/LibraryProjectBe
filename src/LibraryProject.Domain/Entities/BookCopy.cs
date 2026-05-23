using LibraryProject.Domain.Common;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class BookCopy
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public BookCopyStatus Status { get; private set; } = BookCopyStatus.Available;

    public Book Book { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

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
        if (Status != BookCopyStatus.Borrowed)
            throw new DomainValidationException(
                "BOOKCOPY_NOT_BORROWED",
                "Book copy is not currently borrowed.",
                nameof(Status),
                $"Cannot make available a copy with status '{Status}'.");

        Status = BookCopyStatus.Available;
    }
}
