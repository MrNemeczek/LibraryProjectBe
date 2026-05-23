using LibraryProject.Application.Common;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Application.Loans.Exceptions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Loans;

internal sealed class LoanService(
    ILoanRepository loanRepository,
    IUnitOfWork unitOfWork) : ILoanService
{
    public async Task<PaginatedResponse<LoanResponse>> GetMyLoansAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await loanRepository.CountByUserIdAsync(userId, cancellationToken);
        var loans = await loanRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<LoanResponse>(
            loans.Select(MapToResponse).ToList(), page, pageSize, totalCount, totalPages);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await loanRepository.CountAllAsync(cancellationToken);
        var loans = await loanRepository.GetAllAsync(page, pageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<LoanResponse>(
            loans.Select(MapToResponse).ToList(), page, pageSize, totalCount, totalPages);
    }

    public async Task<LoanResponse> GetByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
    {
        var loan = await GetExistingLoanAsync(id, cancellationToken);

        if (loan.UserId != currentUserId && !IsLibrarianOrAdmin(currentUserRole))
            throw new LoanNotFoundException(id);

        return MapToResponse(loan);
    }

    public async Task ReturnAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken)
    {
        var loan = await GetExistingLoanAsync(id, cancellationToken);

        if (loan.UserId != currentUserId && !IsLibrarianOrAdmin(currentUserRole))
            throw new LoanNotFoundException(id);

        if (loan.Status is LoanStatus.Returned)
            throw new DomainRuleViolationException(
                new LibraryProject.Domain.Common.DomainValidationException(
                    "LOAN_ALREADY_RETURNED",
                    "Loan has already been returned.",
                    nameof(loan.Status),
                    "Cannot return a loan that is already returned."));

        loan.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        loan.Status = LoanStatus.Returned;

        loan.BookCopy.Status = BookCopyStatus.Available;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Loan> GetExistingLoanAsync(int id, CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetByIdAsync(id, cancellationToken);
        if (loan is null)
            throw new LoanNotFoundException(id);
        return loan;
    }

    private static LoanResponse MapToResponse(Loan loan)
    {
        return new LoanResponse(
            loan.Id,
            loan.UserId,
            loan.BookCopyId,
            loan.ReservationId,
            loan.LoanDate,
            loan.ReturnDate,
            loan.Status.ToString());
    }

    private static bool IsLibrarianOrAdmin(string role)
    {
        return role is "Librarian" or "Administrator";
    }
}
