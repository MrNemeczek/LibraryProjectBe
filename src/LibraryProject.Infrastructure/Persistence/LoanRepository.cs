using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class LoanRepository(LibraryDbContext dbContext) : ILoanRepository
{
    public async Task<IReadOnlyList<Loan>> GetByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .Include(l => l.BookCopy)
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoanDate).ThenByDescending(l => l.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken)
        => dbContext.Loans.CountAsync(l => l.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Loan>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .Include(l => l.BookCopy)
            .AsNoTracking()
            .OrderByDescending(l => l.LoanDate).ThenByDescending(l => l.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAllAsync(CancellationToken cancellationToken)
        => dbContext.Loans.CountAsync(cancellationToken);

    public Task<Loan?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Loans
            .Include(l => l.BookCopy)
            .SingleOrDefaultAsync(l => l.Id == id, cancellationToken);

    public void Add(Loan loan) => dbContext.Loans.Add(loan);
}
