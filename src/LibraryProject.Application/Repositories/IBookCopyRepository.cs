using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Repositories;

public interface IBookCopyRepository
{
    Task<BookCopy?> GetAvailableCopyAsync(int bookId, CancellationToken cancellationToken = default);
    Task<BookCopy?> GetReservedCopyAsync(int bookId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetExistingInventoryNumbersAsync(IEnumerable<string> inventoryNumbers, CancellationToken cancellationToken = default);
}
