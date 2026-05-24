namespace LibraryProject.Application.Books;

public sealed record BookResponse(
    int Id,
    string Title,
    string Author,
    string Isbn,
    string Description,
    int CategoryId,
    string CategoryName,
    IReadOnlyList<BookCopyResponse> Copies);
