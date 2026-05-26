using AutoFixture;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Repositories;
using LibraryProject.Application.Reservations;
using LibraryProject.Application.Reservations.Exceptions;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Tests.Reservations;

public class ReservationServiceTests
{
    protected readonly IFixture _fixture;
    protected readonly IReservationRepository _reservationRepository;
    protected readonly IUserRepository _userRepository;
    protected readonly IBookRepository _bookRepository;
    protected readonly IBookCopyRepository _bookCopyRepository;
    protected readonly ILoanRepository _loanRepository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IReservationService _sut;

    public ReservationServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Book>(c => c.FromFactory(() =>
        {
            var book = Book.Create("Title", "Author",
                Isbn.Create("978-3-16-148410-0"), null,
                Category.Create("Fiction"));
            typeof(Book).GetProperty("Id")!.SetValue(book, 1);
            return book;
        }));
        _fixture.Customize<CreateReservationRequest>(c => c
            .With(r => r.BookId, 1)
            .With(r => r.PickupDeadlineDays, 3));
        _fixture.Customize<Reservation>(c => c.FromFactory(() =>
        {
            var reservation = Reservation.Create(1, 1, 3);
            typeof(Reservation).GetProperty("Id")!.SetValue(reservation, 1);
            typeof(Reservation).GetProperty("UserId")!.SetValue(reservation, 1);
            typeof(Reservation).GetProperty("BookId")!.SetValue(reservation, 1);
            typeof(Reservation).GetProperty("Book")!.SetValue(reservation,
                Book.Create("Title", "Author",
                    Isbn.Create("978-3-16-148410-0"), null,
                    Category.Create("Fiction")));
            typeof(Reservation).GetProperty("User")!.SetValue(reservation, new User
            {
                Id = 1,
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan.kowalski@example.com",
                Role = UserRole.Reader
            });
            return reservation;
        }));

        _reservationRepository = Substitute.For<IReservationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _bookRepository = Substitute.For<IBookRepository>();
        _bookCopyRepository = Substitute.For<IBookCopyRepository>();
        _loanRepository = Substitute.For<ILoanRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _sut = new ReservationService(
            _reservationRepository, _userRepository, _bookRepository, _bookCopyRepository,
            _loanRepository, _unitOfWork);
    }

    public class CreateAsync : ReservationServiceTests
    {
        [Fact]
        public async Task Should_create_reservation()
        {
            var request = _fixture.Create<CreateReservationRequest>();
            var book = _fixture.Create<Book>();
            var availableCopy = BookCopy.Create("INV-001");

            _bookRepository.GetByIdAsync(request.BookId, Arg.Any<CancellationToken>()).Returns(book);
            _reservationRepository.ExistsActiveByUserAndBookAsync(
                1, request.BookId, Arg.Any<CancellationToken>()).Returns(false);
            _bookCopyRepository.GetAvailableCopyAsync(
                request.BookId, Arg.Any<CancellationToken>()).Returns(availableCopy);
            _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new User
            {
                Id = 1,
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan.kowalski@example.com",
                Role = UserRole.Reader
            });

            var result = await _sut.CreateAsync(request, 1, CancellationToken.None);

            result.Should().NotBeNull();
            result.ReaderFirstName.Should().Be("Jan");
            result.ReaderLastName.Should().Be("Kowalski");
            _reservationRepository.Received(1).Add(Arg.Any<Reservation>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_book_not_found()
        {
            var request = _fixture.Create<CreateReservationRequest>();
            _bookRepository.GetByIdAsync(request.BookId, Arg.Any<CancellationToken>())
                .Returns((Book?)null);

            var act = () => _sut.CreateAsync(request, 1, CancellationToken.None);

            await act.Should().ThrowAsync<ReservationNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_active_reservation_exists()
        {
            var request = _fixture.Create<CreateReservationRequest>();
            var book = _fixture.Create<Book>();

            _bookRepository.GetByIdAsync(request.BookId, Arg.Any<CancellationToken>()).Returns(book);
            _reservationRepository.ExistsActiveByUserAndBookAsync(
                1, request.BookId, Arg.Any<CancellationToken>()).Returns(true);

            var act = () => _sut.CreateAsync(request, 1, CancellationToken.None);

            await act.Should().ThrowAsync<ActiveReservationAlreadyExistsException>();
        }

        [Fact]
        public async Task Should_throw_when_no_available_copy()
        {
            var request = _fixture.Create<CreateReservationRequest>();
            var book = _fixture.Create<Book>();

            _bookRepository.GetByIdAsync(request.BookId, Arg.Any<CancellationToken>()).Returns(book);
            _reservationRepository.ExistsActiveByUserAndBookAsync(
                1, request.BookId, Arg.Any<CancellationToken>()).Returns(false);
            _bookCopyRepository.GetAvailableCopyAsync(
                request.BookId, Arg.Any<CancellationToken>()).Returns((BookCopy?)null);

            var act = () => _sut.CreateAsync(request, 1, CancellationToken.None);

            await act.Should().ThrowAsync<DomainRuleViolationException>()
                .Where(e => e.Code == "NO_AVAILABLE_COPY_FOR_RESERVATION");
        }

        [Fact]
        public async Task Should_use_custom_pickup_deadline()
        {
            var request = _fixture.Build<CreateReservationRequest>()
                .With(r => r.PickupDeadlineDays, 5)
                .Create();
            var book = _fixture.Create<Book>();
            var availableCopy = BookCopy.Create("INV-001");

            _bookRepository.GetByIdAsync(request.BookId, Arg.Any<CancellationToken>()).Returns(book);
            _reservationRepository.ExistsActiveByUserAndBookAsync(
                1, request.BookId, Arg.Any<CancellationToken>()).Returns(false);
            _bookCopyRepository.GetAvailableCopyAsync(
                request.BookId, Arg.Any<CancellationToken>()).Returns(availableCopy);

            var result = await _sut.CreateAsync(request, 1, CancellationToken.None);

            result.Should().NotBeNull();
        }
    }

    public class GetByIdAsync : ReservationServiceTests
    {
        [Fact]
        public async Task Should_return_reservation_when_user_owns_it()
        {
            var reservation = _fixture.Create<Reservation>();
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);

            var result = await _sut.GetByIdAsync(1, 1, "Reader", CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_return_reservation_when_user_is_librarian()
        {
            var reservation = _fixture.Create<Reservation>();
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);

            var result = await _sut.GetByIdAsync(1, 99, "Librarian", CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_throw_when_user_does_not_own_reservation()
        {
            var reservation = _fixture.Create<Reservation>();
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);

            var act = () => _sut.GetByIdAsync(1, 99, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<ReservationNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_reservation_not_found()
        {
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Reservation?)null);

            var act = () => _sut.GetByIdAsync(1, 1, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<ReservationNotFoundException>();
        }
    }

    public class GetMyReservationsAsync : ReservationServiceTests
    {
        [Fact]
        public async Task Should_return_paginated_reservations()
        {
            var reservations = new List<Reservation> { _fixture.Create<Reservation>() };
            _reservationRepository.CountByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(1);
            _reservationRepository.GetByUserIdAsync(1, 1, 20, Arg.Any<CancellationToken>())
                .Returns(reservations);

            var result = await _sut.GetMyReservationsAsync(1, 1, 20, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }
    }

    public class GetAllAsync : ReservationServiceTests
    {
        [Fact]
        public async Task Should_return_all_reservations_paginated()
        {
            var reservations = new List<Reservation> { _fixture.Create<Reservation>() };
            _reservationRepository.CountAllAsync(Arg.Any<GetReservationsRequest>(), Arg.Any<CancellationToken>()).Returns(1);
            _reservationRepository.GetAllAsync(Arg.Any<GetReservationsRequest>(), Arg.Any<CancellationToken>())
                .Returns(reservations);

            var result = await _sut.GetAllAsync(new GetReservationsRequest { Page = 1, PageSize = 20 }, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }
    }

    public class CancelAsync : ReservationServiceTests
    {
        [Fact]
        public async Task Should_cancel_reservation_when_user_owns_it()
        {
            var reservation = _fixture.Create<Reservation>();
            var reservedCopy = BookCopy.Create("INV-001");
            reservedCopy.Reserve();
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);
            _bookCopyRepository.GetReservedCopyAsync(reservation.BookId, Arg.Any<CancellationToken>()).Returns(reservedCopy);

            await _sut.CancelAsync(1, 1, CancellationToken.None);

            reservation.Status.Should().Be(ReservationStatus.Cancelled);
            reservedCopy.Status.Should().Be(BookCopyStatus.Available);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_user_does_not_own_reservation()
        {
            var reservation = _fixture.Create<Reservation>();
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);

            var act = () => _sut.CancelAsync(1, 99, CancellationToken.None);

            await act.Should().ThrowAsync<ReservationNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_reservation_not_found()
        {
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Reservation?)null);

            var act = () => _sut.CancelAsync(1, 1, CancellationToken.None);

            await act.Should().ThrowAsync<ReservationNotFoundException>();
        }
    }

    public class FulfillAsync : ReservationServiceTests
    {
        [Fact]
        public async Task Should_fulfill_reservation_and_create_loan()
        {
            var reservation = _fixture.Create<Reservation>();
            var availableCopy = BookCopy.Create("INV-001");

            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);
            _bookCopyRepository.GetAvailableCopyAsync(
                reservation.BookId, Arg.Any<CancellationToken>()).Returns(availableCopy);

            await _sut.FulfillAsync(1, CancellationToken.None);

            reservation.Status.Should().Be(ReservationStatus.Fulfilled);
            availableCopy.Status.Should().Be(BookCopyStatus.Borrowed);
            _loanRepository.Received(1).Add(Arg.Is<Loan>(l =>
                l.UserId == reservation.UserId &&
                l.BookCopyId == availableCopy.Id &&
                l.ReservationId == reservation.Id));
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_reservation_not_found()
        {
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Reservation?)null);

            var act = () => _sut.FulfillAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<ReservationNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_no_available_copy()
        {
            var reservation = _fixture.Create<Reservation>();
            _reservationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(reservation);
            _bookCopyRepository.GetAvailableCopyAsync(
                reservation.BookId, Arg.Any<CancellationToken>()).Returns((BookCopy?)null);

            var act = () => _sut.FulfillAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<DomainRuleViolationException>()
                .Where(e => e.Code == "NO_AVAILABLE_COPY");
        }
    }
}
