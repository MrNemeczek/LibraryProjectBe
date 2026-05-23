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
}
