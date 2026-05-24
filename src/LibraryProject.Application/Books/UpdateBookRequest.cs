using System.ComponentModel.DataAnnotations;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Books;

public sealed record UpdateBookRequest(
    [Required, MaxLength(Book.MaxTitleLength)] string Title,
    [Required, MaxLength(Book.MaxAuthorLength)] string Author,
    [Required, MaxLength(Isbn.MaxLength)] string Isbn,
    [MaxLength(Book.MaxDescriptionLength)] string? Description,
    [Required, MaxLength(Category.MaxNameLength)] string CategoryName,
    List<string>? InventoryNumbers = null);
