namespace LibraryProject.Application.Reservations;

public sealed record ReservationResponse(
    int Id,
    int UserId,
    int BookId,
    string BookTitle,
    DateOnly ReservationDate,
    DateOnly? PickupDeadline,
    string Status);
