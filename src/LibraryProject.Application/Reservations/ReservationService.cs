using LibraryProject.Application.Common;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Application.Reservations.Exceptions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Reservations;

internal sealed class ReservationService(
    IReservationRepository reservationRepository,
    IBookRepository bookRepository,
    IBookCopyRepository bookCopyRepository,
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork) : IReservationService
{
    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, int userId, CancellationToken cancellationToken)
    {
        var book = await GetExistingBookAsync(request.BookId, cancellationToken);

        var activeExists = await reservationRepository.ExistsActiveByUserAndBookAsync(userId, request.BookId, cancellationToken);
        if (activeExists)
            throw new ActiveReservationAlreadyExistsException(userId, request.BookId);

        var reservation = DomainOperation.Execute(() =>
            Reservation.Create(userId, request.BookId, request.PickupDeadlineDays ?? Reservation.DefaultPickupDeadlineDays));

        reservationRepository.Add(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(reservation, book.Title);
    }

    public async Task<ReservationResponse> GetByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
    {
        var reservation = await GetExistingReservationAsync(id, cancellationToken);

        if (reservation.UserId != currentUserId && !IsLibrarianOrAdmin(currentUserRole))
            throw new ReservationNotFoundException(id);

        return MapToResponse(reservation, reservation.Book.Title);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetMyReservationsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await reservationRepository.CountByUserIdAsync(userId, cancellationToken);
        var reservations = await reservationRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<ReservationResponse>(
            reservations.Select(r => MapToResponse(r, r.Book.Title)).ToList(), page, pageSize, totalCount, totalPages);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await reservationRepository.CountAllAsync(cancellationToken);
        var reservations = await reservationRepository.GetAllAsync(page, pageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<ReservationResponse>(
            reservations.Select(r => MapToResponse(r, r.Book.Title)).ToList(), page, pageSize, totalCount, totalPages);
    }

    public async Task CancelAsync(int id, int userId, CancellationToken cancellationToken)
    {
        var reservation = await GetExistingReservationAsync(id, cancellationToken);

        if (reservation.UserId != userId)
            throw new ReservationNotFoundException(id);

        DomainOperation.Execute(() => reservation.Cancel());
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task FulfillAsync(int id, CancellationToken cancellationToken)
    {
        var reservation = await GetExistingReservationAsync(id, cancellationToken);

        var availableCopy = await bookCopyRepository.GetAvailableCopyAsync(reservation.BookId, cancellationToken);
        if (availableCopy is null)
            throw new DomainRuleViolationException(
                new DomainValidationException(
                    "NO_AVAILABLE_COPY",
                    "No available copy for this book.",
                    nameof(availableCopy),
                    "All copies of this book are currently borrowed or unavailable."));

        DomainOperation.Execute(() => reservation.Fulfill());

        var loan = new Loan
        {
            UserId = reservation.UserId,
            BookCopyId = availableCopy.Id,
            ReservationId = reservation.Id,
            LoanDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = LoanStatus.Active
        };

        availableCopy.Status = BookCopyStatus.Borrowed;

        loanRepository.Add(loan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Book> GetExistingBookAsync(int id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
            throw new Exceptions.ReservationNotFoundException(id);
        return book;
    }

    private async Task<Reservation> GetExistingReservationAsync(int id, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(id, cancellationToken);
        if (reservation is null)
            throw new ReservationNotFoundException(id);
        return reservation;
    }

    private static ReservationResponse MapToResponse(Reservation reservation, string bookTitle)
    {
        return new ReservationResponse(
            reservation.Id,
            reservation.UserId,
            reservation.BookId,
            bookTitle,
            reservation.ReservationDate,
            reservation.PickupDeadline,
            reservation.Status.ToString());
    }

    private static bool IsLibrarianOrAdmin(string role)
    {
        return role is "Librarian" or "Administrator";
    }
}
