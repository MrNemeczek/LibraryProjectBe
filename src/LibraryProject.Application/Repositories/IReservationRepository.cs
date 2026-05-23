using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Repositories;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Reservation>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountAllAsync(CancellationToken cancellationToken);
    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsActiveByUserAndBookAsync(int userId, int bookId, CancellationToken cancellationToken);
    void Add(Reservation reservation);
}
