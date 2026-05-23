using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Books.Exceptions;

public sealed class BookNotFoundException(int id) : ApplicationExceptionBase(
    "BOOK_NOT_FOUND",
    "Book was not found.",
    (int)HttpStatusCode.NotFound,
    new Dictionary<string, string[]>
    {
        [nameof(id)] = ["Book does not exist."]
    })
{
    public int Id { get; } = id;
}
