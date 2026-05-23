using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class ReservationRepository(LibraryDbContext dbContext) : IReservationRepository
{
    public async Task<IReadOnlyList<Reservation>> GetByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .Include(r => r.Book)
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken)
        => dbContext.Reservations.CountAsync(r => r.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Reservations
            .Include(r => r.Book)
            .AsNoTracking()
            .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAllAsync(CancellationToken cancellationToken)
        => dbContext.Reservations.CountAsync(cancellationToken);

    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Reservations
            .Include(r => r.Book)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExistsActiveByUserAndBookAsync(int userId, int bookId, CancellationToken cancellationToken)
        => dbContext.Reservations.AnyAsync(
            r => r.UserId == userId && r.BookId == bookId && r.Status == ReservationStatus.Active,
            cancellationToken);

    public void Add(Reservation reservation) => dbContext.Reservations.Add(reservation);
}
