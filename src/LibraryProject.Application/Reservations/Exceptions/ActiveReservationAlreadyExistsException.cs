using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Reservations.Exceptions;

public sealed class ActiveReservationAlreadyExistsException(int userId, int bookId)
    : ApplicationExceptionBase(
        "ACTIVE_RESERVATION_ALREADY_EXISTS",
        "You already have an active reservation for this book.",
        (int)HttpStatusCode.Conflict,
        new Dictionary<string, string[]>
        {
            [nameof(bookId)] = ["An active reservation for this book already exists."]
        })
{
    public int UserId { get; } = userId;
    public int BookId { get; } = bookId;
}
