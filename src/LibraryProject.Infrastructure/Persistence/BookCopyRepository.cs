using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class BookCopyRepository(LibraryDbContext dbContext) : IBookCopyRepository
{
    public Task<BookCopy?> GetAvailableCopyAsync(int bookId, CancellationToken cancellationToken)
    {
        return dbContext.BookCopies
            .FirstOrDefaultAsync(c => c.BookId == bookId && c.Status == BookCopyStatus.Available, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetExistingInventoryNumbersAsync(IEnumerable<string> inventoryNumbers, CancellationToken cancellationToken)
    {
        return await dbContext.BookCopies
            .Where(c => inventoryNumbers.Contains(c.InventoryNumber))
            .Select(c => c.InventoryNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<BookCopy?> GetReservedCopyAsync(int bookId, CancellationToken cancellationToken = default)
    {
        return await dbContext.BookCopies
            .FirstOrDefaultAsync(c => c.BookId == bookId && c.Status == BookCopyStatus.Reserved, cancellationToken);
    }
}
