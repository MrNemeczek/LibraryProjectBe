using FluentAssertions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Tests.Domain;

public class BookEntityTests
{
    private static readonly Category DefaultCategory = Category.Create("Fiction");
    private static readonly Isbn DefaultIsbn = Isbn.Create("978-3-16-148410-0");

    [Fact]
    public void Create_should_set_properties()
    {
        var book = Book.Create("Title", "Author", DefaultIsbn, "Description", DefaultCategory);

        book.Title.Should().Be("Title");
        book.Author.Should().Be("Author");
        book.Isbn.Should().Be(DefaultIsbn);
        book.Description.Should().Be("Description");
        book.Category.Should().Be(DefaultCategory);
        book.IsDeleted.Should().BeFalse();
        book.Copies.Should().BeEmpty();
    }

    [Fact]
    public void Create_should_throw_when_title_is_null()
    {
        var act = () => Book.Create(null, "Author", DefaultIsbn, "Description", DefaultCategory);
        act.Should().Throw<DomainValidationException>().Which.Code.Should().Be("BOOK_TITLE_REQUIRED");
    }

    [Fact]
    public void Create_should_throw_when_author_is_null()
    {
        var act = () => Book.Create("Title", null, DefaultIsbn, "Description", DefaultCategory);
        act.Should().Throw<DomainValidationException>().Which.Code.Should().Be("BOOK_AUTHOR_REQUIRED");
    }

    [Fact]
    public void UpdateDetails_should_update_properties()
    {
        var book = Book.Create("Old Title", "Old Author", DefaultIsbn, "Old Desc", DefaultCategory);
        var newCategory = Category.Create("NonFiction");
        var newIsbn = Isbn.Create("978-0-545-01022-1");

        book.UpdateDetails("New Title", "New Author", newIsbn, "New Desc", newCategory);

        book.Title.Should().Be("New Title");
        book.Author.Should().Be("New Author");
        book.Isbn.Should().Be(newIsbn);
        book.Description.Should().Be("New Desc");
        book.Category.Should().Be(newCategory);
    }

    [Fact]
    public void AddCopy_should_add_copy_to_collection()
    {
        var book = Book.Create("Title", "Author", DefaultIsbn, "Description", DefaultCategory);

        book.AddCopy("INV-001");

        book.Copies.Should().ContainSingle(c => c.InventoryNumber == "INV-001");
    }

    [Fact]
    public void AddCopy_should_throw_when_duplicate_inventory_number()
    {
        var book = Book.Create("Title", "Author", DefaultIsbn, "Description", DefaultCategory);
        book.AddCopy("INV-001");

        var act = () => book.AddCopy("INV-001");
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("BOOK_DUPLICATE_COPY_INVENTORY_NUMBER");
    }

    [Fact]
    public void Delete_should_mark_as_deleted()
    {
        var book = Book.Create("Title", "Author", DefaultIsbn, "Description", DefaultCategory);
        var now = new DateTime(2026, 5, 24, 12, 0, 0, DateTimeKind.Utc);

        book.Delete(now);

        book.IsDeleted.Should().BeTrue();
        book.DeletedAt.Should().Be(now);
    }

    [Fact]
    public void Delete_called_twice_should_not_change_DeletedAt()
    {
        var book = Book.Create("Title", "Author", DefaultIsbn, "Description", DefaultCategory);
        book.Delete(new DateTime(2026, 5, 24, 12, 0, 0, DateTimeKind.Utc));
        var later = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        book.Delete(later);

        book.DeletedAt.Should().NotBe(later);
    }
}
