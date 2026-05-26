using LibraryProject.Application.Repositories;
using LibraryProject.Application.Books;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class BookRepository(LibraryDbContext dbContext) : IBookRepository
{
    public async Task<IReadOnlyList<Book>> GetAsync(GetBooksRequest request, CancellationToken cancellationToken)
    {
        return await CreateFilteredQuery(request)
            .Include(book => book.Category)
            .Include(book => book.Copies)
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(GetBooksRequest request, CancellationToken cancellationToken)
    {
        return CreateFilteredQuery(request).CountAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Books
            .Include(book => book.Category)
            .Include(book => book.Copies)
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

    private IQueryable<Book> CreateFilteredQuery(GetBooksRequest request)
    {
        var query = dbContext.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = request.Title.Trim();
            query = query.Where(book => book.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            var author = request.Author.Trim();
            query = query.Where(book => book.Author.Contains(author));
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(book => book.CategoryId == request.CategoryId.Value);
        }

        return query;
    }
}
