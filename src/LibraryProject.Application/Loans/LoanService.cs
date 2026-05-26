using LibraryProject.Application.Common;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Application.Loans.Exceptions;
using LibraryProject.Domain.Entities;

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

    public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(GetLoansRequest request, CancellationToken cancellationToken)
    {
        var totalCount = await loanRepository.CountAllAsync(request, cancellationToken);
        var loans = await loanRepository.GetAllAsync(request, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PaginatedResponse<LoanResponse>(
            loans.Select(MapToResponse).ToList(), request.Page, request.PageSize, totalCount, totalPages);
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

        if (!IsLibrarianOrAdmin(currentUserRole))
            throw new LoanNotFoundException(id);

        DomainOperation.Execute(() => loan.Return());

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
            loan.User.FirstName,
            loan.User.LastName,
            loan.BookCopyId,
            loan.BookCopy.InventoryNumber,
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
