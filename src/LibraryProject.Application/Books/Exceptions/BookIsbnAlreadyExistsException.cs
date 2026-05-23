using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Books.Exceptions;

public sealed class BookIsbnAlreadyExistsException(string isbn) : ApplicationExceptionBase(
    "BOOK_ISBN_ALREADY_EXISTS",
    "Book with this ISBN already exists.",
    (int)HttpStatusCode.Conflict,
    new Dictionary<string, string[]>
    {
        [nameof(isbn)] = ["ISBN is already assigned to another book."]
    })
{
    public string Isbn { get; } = isbn;
}
