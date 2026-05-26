using LibraryProject.Domain.Entities;
using LibraryProject.Application.Loans;

namespace LibraryProject.Application.Repositories;

public interface ILoanRepository
{
    Task<IReadOnlyList<Loan>> GetByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> GetAllAsync(GetLoansRequest request, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(GetLoansRequest request, CancellationToken cancellationToken = default);
    Task<Loan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Loan loan);
}
