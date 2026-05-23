namespace LibraryProject.Application.Loans;

public sealed record LoanResponse(
    int Id,
    int UserId,
    int BookCopyId,
    int? ReservationId,
    DateOnly LoanDate,
    DateOnly? ReturnDate,
    string Status);
