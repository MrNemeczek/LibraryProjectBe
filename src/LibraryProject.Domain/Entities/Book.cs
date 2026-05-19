namespace LibraryProject.Domain.Entities;

public sealed class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
