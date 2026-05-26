namespace LibraryProject.Application.Reservations;

public sealed record ReservationResponse(
    int Id,
    string ReaderFirstName,
    string ReaderLastName,
    int BookId,
    string BookTitle,
    DateOnly ReservationDate,
    DateOnly? PickupDeadline,
    string Status);
