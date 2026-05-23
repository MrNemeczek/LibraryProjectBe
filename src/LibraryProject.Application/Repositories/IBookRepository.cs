using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Repositories;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByIsbnAsync(Isbn isbn, int? excludedBookId, CancellationToken cancellationToken);
    void Add(Book book);
}
