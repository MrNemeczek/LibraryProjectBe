using LibraryProject.Application.Repositories;
using LibraryProject.Application.Reservations;
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
            .Include(r => r.User)
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken)
        => dbContext.Reservations.CountAsync(r => r.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(GetReservationsRequest request, CancellationToken cancellationToken)
    {
        return await CreateFilteredQuery(request)
            .Include(r => r.Book)
            .Include(r => r.User)
            .AsNoTracking()
            .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAllAsync(GetReservationsRequest request, CancellationToken cancellationToken)
        => CreateFilteredQuery(request).CountAsync(cancellationToken);

    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Reservations
            .Include(r => r.Book)
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExistsActiveByUserAndBookAsync(int userId, int bookId, CancellationToken cancellationToken)
        => dbContext.Reservations.AnyAsync(
            r => r.UserId == userId && r.BookId == bookId && r.Status == ReservationStatus.Active,
            cancellationToken);

    public void Add(Reservation reservation) => dbContext.Reservations.Add(reservation);

    private IQueryable<Reservation> CreateFilteredQuery(GetReservationsRequest request)
    {
        var query = dbContext.Reservations.AsQueryable();

        if (request.ReservationId is not null)
        {
            query = query.Where(reservation => reservation.Id == request.ReservationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ReaderName))
        {
            var readerName = request.ReaderName.Trim();
            query = query.Where(reservation =>
                reservation.User.FirstName.Contains(readerName) ||
                reservation.User.LastName.Contains(readerName) ||
                (reservation.User.FirstName + " " + reservation.User.LastName).Contains(readerName));
        }

        return query;
    }
}
