using LibraryProject.Application.Repositories;
using LibraryProject.Application.Loans;
using LibraryProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class LoanRepository(LibraryDbContext dbContext) : ILoanRepository
{
    public async Task<IReadOnlyList<Loan>> GetByUserIdAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Loans
            .Include(l => l.BookCopy)
            .Include(l => l.User)
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoanDate).ThenByDescending(l => l.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken)
        => dbContext.Loans.CountAsync(l => l.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Loan>> GetAllAsync(GetLoansRequest request, CancellationToken cancellationToken)
    {
        return await CreateFilteredQuery(request)
            .Include(l => l.BookCopy)
            .Include(l => l.User)
            .AsNoTracking()
            .OrderByDescending(l => l.LoanDate).ThenByDescending(l => l.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAllAsync(GetLoansRequest request, CancellationToken cancellationToken)
        => CreateFilteredQuery(request).CountAsync(cancellationToken);

    public Task<Loan?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Loans
            .Include(l => l.BookCopy)
            .Include(l => l.User)
            .SingleOrDefaultAsync(l => l.Id == id, cancellationToken);

    public void Add(Loan loan) => dbContext.Loans.Add(loan);

    private IQueryable<Loan> CreateFilteredQuery(GetLoansRequest request)
    {
        var query = dbContext.Loans.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ReaderName))
        {
            var readerName = request.ReaderName.Trim();
            query = query.Where(loan =>
                loan.User.FirstName.Contains(readerName) ||
                loan.User.LastName.Contains(readerName) ||
                (loan.User.FirstName + " " + loan.User.LastName).Contains(readerName));
        }

        if (!string.IsNullOrWhiteSpace(request.BookCopyInventoryNumber))
        {
            var inventoryNumber = request.BookCopyInventoryNumber.Trim();
            query = query.Where(loan => loan.BookCopy.InventoryNumber.Contains(inventoryNumber));
        }

        return query;
    }
}
