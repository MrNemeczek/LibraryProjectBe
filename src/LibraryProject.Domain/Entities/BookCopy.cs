using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class BookCopy
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    public Book Book { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
