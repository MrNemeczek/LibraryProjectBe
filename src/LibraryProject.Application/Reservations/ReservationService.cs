using LibraryProject.Application.Common;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Application.Reservations.Exceptions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Reservations;

internal sealed class ReservationService(
    IReservationRepository reservationRepository,
    IUserRepository userRepository,
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

        var availableCopy = await bookCopyRepository.GetAvailableCopyAsync(request.BookId, cancellationToken);
        if (availableCopy is null)
            throw new DomainRuleViolationException(
                new DomainValidationException(
                    "NO_AVAILABLE_COPY_FOR_RESERVATION",
                    "No available copy of this book for reservation.",
                    nameof(request.BookId),
                    "All copies of this book are currently borrowed or unavailable."));

        var reservation = DomainOperation.Execute(() =>
            Reservation.Create(userId, request.BookId, request.PickupDeadlineDays ?? Reservation.DefaultPickupDeadlineDays));

        DomainOperation.Execute(availableCopy.Reserve);

        reservationRepository.Add(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return MapToResponse(reservation, book.Title, user?.FirstName ?? string.Empty, user?.LastName ?? string.Empty);
    }

    public async Task<ReservationResponse> GetByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
    {
        var reservation = await GetExistingReservationAsync(id, cancellationToken);

        if (reservation.UserId != currentUserId && !IsLibrarianOrAdmin(currentUserRole))
            throw new ReservationNotFoundException(id);

        return MapToResponse(reservation);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetMyReservationsAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await reservationRepository.CountByUserIdAsync(userId, cancellationToken);
        var reservations = await reservationRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<ReservationResponse>(
            reservations.Select(MapToResponse).ToList(), page, pageSize, totalCount, totalPages);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetAllAsync(GetReservationsRequest request, CancellationToken cancellationToken)
    {
        var totalCount = await reservationRepository.CountAllAsync(request, cancellationToken);
        var reservations = await reservationRepository.GetAllAsync(request, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PaginatedResponse<ReservationResponse>(
            reservations.Select(MapToResponse).ToList(), request.Page, request.PageSize, totalCount, totalPages);
    }

    public async Task CancelAsync(int id, int userId, CancellationToken cancellationToken)
    {
        var reservation = await GetExistingReservationAsync(id, cancellationToken);

        if (reservation.UserId != userId)
            throw new ReservationNotFoundException(id);

        var reservedCopy = await bookCopyRepository.GetReservedCopyAsync(reservation.BookId, cancellationToken);
        if (reservedCopy is null)
        {
            throw new DomainRuleViolationException(
                new DomainValidationException(
                    "RESERVED_COPY_NOT_FOUND",
                    "Reserved copy for this reservation not found.",
                    nameof(reservation.BookId),
                    "The reserved copy associated with this reservation could not be found."));
        }

        DomainOperation.Execute(reservation.Cancel);
        DomainOperation.Execute(reservedCopy.MakeAvailable);

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
        DomainOperation.Execute(() => availableCopy.Borrow());

        var loan = DomainOperation.Execute(() =>
            Loan.Create(reservation.UserId, availableCopy.Id, reservation.Id));

        loanRepository.Add(loan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Book> GetExistingBookAsync(int id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
            throw new ReservationNotFoundException(id);
        return book;
    }

    private async Task<Reservation> GetExistingReservationAsync(int id, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(id, cancellationToken);
        if (reservation is null)
            throw new ReservationNotFoundException(id);
        return reservation;
    }

    private static ReservationResponse MapToResponse(Reservation reservation)
    {
        return MapToResponse(reservation, reservation.Book.Title, reservation.User.FirstName, reservation.User.LastName);
    }

    private static ReservationResponse MapToResponse(Reservation reservation, string bookTitle, string readerFirstName, string readerLastName)
    {
        return new ReservationResponse(
            reservation.Id,
            readerFirstName,
            readerLastName,
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
