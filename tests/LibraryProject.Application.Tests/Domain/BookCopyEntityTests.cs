using FluentAssertions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;

namespace LibraryProject.Application.Tests.Domain;

public class BookCopyEntityTests
{
    [Fact]
    public void Create_should_set_inventory_number_and_status_available()
    {
        var copy = BookCopy.Create("INV-001");

        copy.InventoryNumber.Should().Be("INV-001");
        copy.Status.Should().Be(BookCopyStatus.Available);
    }

    [Fact]
    public void Create_should_throw_when_inventory_number_is_null()
    {
        var act = () => BookCopy.Create(null!);
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("BOOKCOPY_INVENTORYNUMBER_REQUIRED");
    }

    [Fact]
    public void Borrow_should_change_status_to_borrowed()
    {
        var copy = BookCopy.Create("INV-001");

        copy.Borrow();

        copy.Status.Should().Be(BookCopyStatus.Borrowed);
    }

    [Fact]
    public void Borrow_should_throw_when_not_available()
    {
        var copy = BookCopy.Create("INV-001");
        copy.Borrow();

        var act = () => copy.Borrow();
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("BOOKCOPY_NOT_AVAILABLE");
    }

    [Fact]
    public void MakeAvailable_should_change_status_to_available()
    {
        var copy = BookCopy.Create("INV-001");
        copy.Borrow();

        copy.MakeAvailable();

        copy.Status.Should().Be(BookCopyStatus.Available);
    }

    [Fact]
    public void MakeAvailable_should_throw_when_not_borrowed()
    {
        var copy = BookCopy.Create("INV-001");

        var act = () => copy.MakeAvailable();
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("BOOKCOPY_NOT_BORROWED");
    }
}
