using LibraryProject.Application.Common.Pagination;

namespace LibraryProject.Application.Books;

public sealed class GetBooksRequest : PaginationRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int? CategoryId { get; set; }
}
