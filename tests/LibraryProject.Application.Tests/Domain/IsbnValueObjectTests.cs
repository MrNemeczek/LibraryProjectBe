using FluentAssertions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Tests.Domain;

public class IsbnValueObjectTests
{
    [Fact]
    public void Create_should_return_isbn_with_value()
    {
        var isbn = Isbn.Create("978-3-16-148410-0");
        isbn.Value.Should().Be("978-3-16-148410-0");
    }

    [Fact]
    public void Create_should_trim_value()
    {
        var isbn = Isbn.Create("  978-3-16-148410-0  ");
        isbn.Value.Should().Be("978-3-16-148410-0");
    }

    [Fact]
    public void Create_should_throw_when_value_is_null()
    {
        var act = () => Isbn.Create(null);
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("BOOK_VALUE_REQUIRED");
    }

    [Fact]
    public void Create_should_throw_when_value_exceeds_max_length()
    {
        var act = () => Isbn.Create(new string('x', 21));
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("BOOK_VALUE_TOO_LONG");
    }

    [Fact]
    public void ToString_should_return_value()
    {
        var isbn = Isbn.Create("978-3-16-148410-0");
        isbn.ToString().Should().Be("978-3-16-148410-0");
    }
}
