using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Reservations.Exceptions;

public sealed class ReservationNotFoundException(int id) : ApplicationExceptionBase(
    "RESERVATION_NOT_FOUND", "Reservation was not found.",
    (int)HttpStatusCode.NotFound,
    new Dictionary<string, string[]> { [nameof(id)] = ["Reservation does not exist."] })
{
    public int Id { get; } = id;
}
