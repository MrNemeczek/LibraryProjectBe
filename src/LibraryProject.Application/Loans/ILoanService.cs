using LibraryProject.Application.Common.Pagination;

namespace LibraryProject.Application.Loans;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetMyLoansAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<LoanResponse>> GetAllAsync(GetLoansRequest request, CancellationToken cancellationToken = default);
    Task<LoanResponse> GetByIdAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default);
    Task ReturnAsync(int id, int currentUserId, string currentUserRole, CancellationToken cancellationToken = default);
}
