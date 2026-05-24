namespace LibraryProject.Application.Books;

public sealed record BookCopyResponse(
    int Id,
    string InventoryNumber,
    string Status);
