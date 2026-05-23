using LibraryProject.Domain.Common;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class Loan
{
    private Loan()
    {
    }

    private Loan(int userId, int bookCopyId, int? reservationId, DateOnly loanDate)
    {
        UserId = userId;
        BookCopyId = bookCopyId;
        ReservationId = reservationId;
        LoanDate = loanDate;
        Status = LoanStatus.Active;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int BookCopyId { get; private set; }
    public int? ReservationId { get; private set; }
    public DateOnly LoanDate { get; private set; }
    public DateOnly? ReturnDate { get; private set; }
    public LoanStatus Status { get; private set; }

    public User User { get; private set; } = null!;
    public BookCopy BookCopy { get; private set; } = null!;
    public Reservation? Reservation { get; private set; }

    public static Loan Create(int userId, int bookCopyId, int? reservationId)
    {
        return new Loan(userId, bookCopyId, reservationId, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public void Return()
    {
        if (Status == LoanStatus.Returned)
            throw new DomainValidationException(
                "LOAN_ALREADY_RETURNED",
                "Loan has already been returned.",
                nameof(Status),
                "Cannot return a loan that is already returned.");

        ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        Status = LoanStatus.Returned;

        BookCopy.MakeAvailable();
    }
}
