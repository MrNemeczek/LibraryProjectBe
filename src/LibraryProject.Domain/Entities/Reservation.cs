using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public DateOnly ReservationDate { get; set; }
    public DateOnly? PickupDeadline { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
