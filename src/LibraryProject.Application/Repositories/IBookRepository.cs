using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;
using LibraryProject.Application.Books;

namespace LibraryProject.Application.Repositories;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAsync(GetBooksRequest request, CancellationToken cancellationToken);
    Task<int> CountAsync(GetBooksRequest request, CancellationToken cancellationToken);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Book?> GetByIdWithCopiesAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByIsbnAsync(Isbn isbn, int? excludedBookId, CancellationToken cancellationToken);
    void Add(Book book);
}
