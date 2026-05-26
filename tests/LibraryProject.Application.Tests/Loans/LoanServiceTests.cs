using AutoFixture;
using FluentAssertions;
using LibraryProject.Application.Loans;
using LibraryProject.Application.Loans.Exceptions;
using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using NSubstitute;

namespace LibraryProject.Application.Tests.Loans;

public class LoanServiceTests
{
    protected readonly IFixture _fixture;
    protected readonly ILoanRepository _loanRepository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly ILoanService _sut;

    public LoanServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Loan>(c => c.FromFactory(() =>
        {
            var copy = BookCopy.Create("INV-001");
            copy.Borrow();
            var loan = Loan.Create(1, copy.Id, null);
            var user = new User { Id = 1, FirstName = "Jan", LastName = "Kowalski" };
            typeof(Loan).GetProperty("Id")!.SetValue(loan, 1);
            typeof(Loan).GetProperty("UserId")!.SetValue(loan, 1);
            typeof(Loan).GetProperty("BookCopy")!.SetValue(loan, copy);
            typeof(Loan).GetProperty("User")!.SetValue(loan, user);
            return loan;
        }));

        _loanRepository = Substitute.For<ILoanRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _sut = new LoanService(_loanRepository, _unitOfWork);
    }

    public class GetMyLoansAsync : LoanServiceTests
    {
        [Fact]
        public async Task Should_return_paginated_loans_for_user()
        {
            var loans = new List<Loan> { _fixture.Create<Loan>() };
            _loanRepository.CountByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(1);
            _loanRepository.GetByUserIdAsync(1, 1, 20, Arg.Any<CancellationToken>()).Returns(loans);

            var result = await _sut.GetMyLoansAsync(1, 1, 20, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Should_return_empty_when_no_loans()
        {
            _loanRepository.CountByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(0);
            _loanRepository.GetByUserIdAsync(1, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<Loan>());

            var result = await _sut.GetMyLoansAsync(1, 1, 20, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }

    public class GetAllAsync : LoanServiceTests
    {
        [Fact]
        public async Task Should_return_all_loans_paginated()
        {
            var loans = new List<Loan> { _fixture.Create<Loan>() };
            var request = new GetLoansRequest { Page = 1, PageSize = 20 };
            _loanRepository.CountAllAsync(request, Arg.Any<CancellationToken>()).Returns(1);
            _loanRepository.GetAllAsync(request, Arg.Any<CancellationToken>()).Returns(loans);

            var result = await _sut.GetAllAsync(request, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }
    }

    public class GetByIdAsync : LoanServiceTests
    {
        [Fact]
        public async Task Should_return_loan_when_user_owns_it()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            var result = await _sut.GetByIdAsync(1, 1, "Reader", CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_return_loan_when_user_is_librarian()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            var result = await _sut.GetByIdAsync(1, 99, "Librarian", CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_return_loan_when_user_is_administrator()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            var result = await _sut.GetByIdAsync(1, 99, "Administrator", CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_throw_when_user_does_not_own_loan_and_is_not_librarian_or_admin()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            var act = () => _sut.GetByIdAsync(1, 99, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<LoanNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_loan_not_found()
        {
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Loan?)null);

            var act = () => _sut.GetByIdAsync(1, 1, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<LoanNotFoundException>();
        }
    }

    public class ReturnAsync : LoanServiceTests
    {
        [Fact]
        public async Task Should_throw_when_reader_returns_own_loan()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            var act = () => _sut.ReturnAsync(1, 1, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<LoanNotFoundException>();
        }

        [Fact]
        public async Task Should_return_loan_when_librarian_returns_for_user()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            await _sut.ReturnAsync(1, 99, "Librarian", CancellationToken.None);

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_return_loan_when_administrator_returns_for_user()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            await _sut.ReturnAsync(1, 99, "Administrator", CancellationToken.None);

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_user_does_not_own_loan()
        {
            var loan = _fixture.Create<Loan>();
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(loan);

            var act = () => _sut.ReturnAsync(1, 99, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<LoanNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_loan_not_found()
        {
            _loanRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Loan?)null);

            var act = () => _sut.ReturnAsync(1, 1, "Reader", CancellationToken.None);

            await act.Should().ThrowAsync<LoanNotFoundException>();
        }
    }
}
