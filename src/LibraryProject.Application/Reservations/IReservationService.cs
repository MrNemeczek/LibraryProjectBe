using LibraryProject.Application.Common.Pagination;

namespace LibraryProject.Application.Reservations;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, int userId, CancellationToken cancellationToken);
    Task<ReservationResponse> GetByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken);
    Task<PaginatedResponse<ReservationResponse>> GetMyReservationsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<PaginatedResponse<ReservationResponse>> GetAllAsync(GetReservationsRequest request, CancellationToken cancellationToken);
    Task CancelAsync(int id, int userId, CancellationToken cancellationToken);
    Task FulfillAsync(int id, CancellationToken cancellationToken);
}
