using LibraryProject.Application.Common.Pagination;

namespace LibraryProject.Application.Reservations;

public sealed class GetReservationsRequest : PaginationRequest
{
    public int? ReservationId { get; set; }
    public string? ReaderName { get; set; }
}
