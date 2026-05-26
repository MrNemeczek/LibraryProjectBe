using LibraryProject.Application.Common.Pagination;

namespace LibraryProject.Application.Books;

public interface IBookService
{
    Task<PaginatedResponse<BookResponse>> GetAsync(GetBooksRequest request, CancellationToken cancellationToken);
    Task<BookResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken);
    Task<BookResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
