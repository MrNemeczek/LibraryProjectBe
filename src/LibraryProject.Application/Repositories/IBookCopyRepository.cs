using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Repositories;

public interface IBookCopyRepository
{
    Task<BookCopy?> GetAvailableCopyAsync(int bookId, CancellationToken cancellationToken = default);
}
