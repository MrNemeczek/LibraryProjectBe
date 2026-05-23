using LibraryProject.Domain.Common;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Domain.Entities;

public sealed class Reservation
{
    public const int DefaultPickupDeadlineDays = 3;
    public const int MaxPickupDeadlineDays = 7;

    private Reservation()
    {
    }

    private Reservation(int userId, int bookId, DateOnly reservationDate, DateOnly pickupDeadline)
    {
        UserId = userId;
        BookId = bookId;
        ReservationDate = reservationDate;
        PickupDeadline = pickupDeadline;
        Status = ReservationStatus.Active;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int BookId { get; private set; }
    public DateOnly ReservationDate { get; private set; }
    public DateOnly? PickupDeadline { get; private set; }
    public ReservationStatus Status { get; private set; }

    public User User { get; private set; } = null!;
    public Book Book { get; private set; } = null!;

    public static Reservation Create(int userId, int bookId, int pickupDeadlineDays = DefaultPickupDeadlineDays)
    {
        if (pickupDeadlineDays < 1 || pickupDeadlineDays > MaxPickupDeadlineDays)
            throw new DomainValidationException(
                "RESERVATION_INVALID_PICKUP_DEADLINE",
                "Pickup deadline days is invalid.",
                nameof(pickupDeadlineDays),
                $"Pickup deadline days must be between 1 and {MaxPickupDeadlineDays}.");
        var reservationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var pickupDeadline = reservationDate.AddDays(pickupDeadlineDays);
        return new Reservation(userId, bookId, reservationDate, pickupDeadline);
    }

    public void Cancel()
    {
        EnsureActive();
        Status = ReservationStatus.Cancelled;
    }

    public void Fulfill()
    {
        EnsureActive();
        Status = ReservationStatus.Fulfilled;
    }

    public void Expire()
    {
        EnsureActive();
        Status = ReservationStatus.Expired;
    }

    private void EnsureActive()
    {
        if (Status != ReservationStatus.Active)
            throw new DomainValidationException(
                "RESERVATION_NOT_ACTIVE",
                "Reservation is not active.",
                nameof(Status),
                $"Cannot modify a reservation with status '{Status}'.");
    }
}
