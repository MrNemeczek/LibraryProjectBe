using LibraryProject.Application.Common.Pagination;

namespace LibraryProject.Application.Loans;

public sealed class GetLoansRequest : PaginationRequest
{
    public string? ReaderName { get; set; }
    public string? BookCopyInventoryNumber { get; set; }
}
