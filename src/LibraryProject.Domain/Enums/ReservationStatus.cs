namespace LibraryProject.Domain.Enums;

/// <summary>
/// Describes the current state of a book reservation.
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// The reservation is active and waiting to be fulfilled or cancelled.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The reservation has been completed, usually by creating a loan.
    /// </summary>
    Fulfilled = 2,

    /// <summary>
    /// The reservation was cancelled before being fulfilled.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// The reservation expired because it was not picked up in time.
    /// </summary>
    Expired = 4
}
