using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class BookRepository(LibraryDbContext dbContext) : IBookRepository
{
    public async Task<IReadOnlyList<Book>> GetAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .Include(book => book.Category)
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return dbContext.Books.CountAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Books
            .Include(book => book.Category)
            .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public Task<Book?> GetByIdWithCopiesAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Books
            .Include(book => book.Copies)
            .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByIsbnAsync(Isbn isbn, int? excludedBookId, CancellationToken cancellationToken)
    {
        var query = dbContext.Books.Where(book => book.Isbn == isbn);

        if (excludedBookId is not null)
        {
            query = query.Where(book => book.Id != excludedBookId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public void Add(Book book)
    {
        dbContext.Books.Add(book);
    }
}
