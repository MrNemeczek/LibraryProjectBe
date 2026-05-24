using FluentAssertions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Tests.Domain;

public class LoanEntityTests
{
    private static Loan CreateLoanWithBookCopy(int userId = 1, int? reservationId = null)
    {
        var copy = BookCopy.Create("INV-001");
        copy.Borrow();
        var loan = Loan.Create(userId, copy.Id, reservationId);
        typeof(Loan).GetProperty("BookCopy")!.SetValue(loan, copy);
        return loan;
    }

    [Fact]
    public void Create_should_set_properties_and_status_active()
    {
        var loan = Loan.Create(1, 2, null);

        loan.UserId.Should().Be(1);
        loan.BookCopyId.Should().Be(2);
        loan.ReservationId.Should().BeNull();
        loan.LoanDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
        loan.Status.Should().Be(LoanStatus.Active);
        loan.ReturnDate.Should().BeNull();
    }

    [Fact]
    public void Create_should_set_reservation_id_when_provided()
    {
        var loan = Loan.Create(1, 2, 5);

        loan.ReservationId.Should().Be(5);
    }

    [Fact]
    public void Return_should_set_status_to_returned_and_set_return_date()
    {
        var loan = CreateLoanWithBookCopy();

        loan.Return();

        loan.Status.Should().Be(LoanStatus.Returned);
        loan.ReturnDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public void Return_should_throw_when_already_returned()
    {
        var loan = CreateLoanWithBookCopy();
        loan.Return();

        var act = () => loan.Return();
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("LOAN_ALREADY_RETURNED");
    }

    [Fact]
    public void Return_should_call_MakeAvailable_on_book_copy()
    {
        // Arrange: need a real BookCopy attached to the loan via the BookCopy property
        var copy = BookCopy.Create("INV-001");
        copy.Borrow();
        var loan = Loan.Create(1, copy.Id, null);

        // Use reflection to set BookCopy since it's normally set by EF
        var bookCopyField = typeof(Loan).GetProperty("BookCopy");
        bookCopyField!.SetValue(loan, copy);

        // Act
        loan.Return();

        // Assert: the copy should now be available again
        copy.Status.Should().Be(BookCopyStatus.Available);
    }
}
