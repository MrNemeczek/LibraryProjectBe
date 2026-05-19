using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookCopyId { get; set; }
    public int? ReservationId { get; set; }
    public DateOnly LoanDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public User User { get; set; } = null!;
    public BookCopy BookCopy { get; set; } = null!;
    public Reservation? Reservation { get; set; }
}
