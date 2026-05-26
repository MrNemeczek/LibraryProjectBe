namespace LibraryProject.Application.Loans;

public sealed record LoanResponse(
    int Id,
    int UserId,
    string ReaderFirstName,
    string ReaderLastName,
    int BookCopyId,
    string BookCopyInventoryNumber,
    int? ReservationId,
    DateOnly LoanDate,
    DateOnly? ReturnDate,
    string Status);
