using FluentAssertions;
using LibraryProject.Domain.Common;

namespace LibraryProject.Application.Tests.Common;

public class GuardTests
{
    public class Required
    {
        [Theory]
        [InlineData("hello")]
        [InlineData("   hello   ")]
        public void Should_trim_and_return_value(string input)
        {
            var result = Guard.Required(input, "Field", 100);
            result.Should().Be("hello");
        }

        [Fact]
        public void Should_throw_when_value_is_null()
        {
            var act = () => Guard.Required(null, "Field", 100);
            act.Should().Throw<DomainValidationException>()
                .Which.Code.Should().Be("FIELD_REQUIRED");
        }

        [Fact]
        public void Should_throw_when_value_is_whitespace()
        {
            var act = () => Guard.Required("   ", "Field", 100);
            act.Should().Throw<DomainValidationException>()
                .Which.Code.Should().Be("FIELD_REQUIRED");
        }

        [Fact]
        public void Should_throw_when_value_exceeds_max_length()
        {
            var act = () => Guard.Required("a".PadRight(101, 'a'), "Field", 100);
            act.Should().Throw<DomainValidationException>()
                .Which.Code.Should().Be("FIELD_TOO_LONG");
        }

        [Fact]
        public void Should_use_prefix_in_error_code()
        {
            var act = () => Guard.Required(null, "Title", 100, "BOOK");
            act.Should().Throw<DomainValidationException>()
                .Which.Code.Should().Be("BOOK_TITLE_REQUIRED");
        }
    }

    public class Optional
    {
        [Fact]
        public void Should_return_empty_string_when_null()
        {
            var result = Guard.Optional(null, "Field", 100);
            result.Should().BeEmpty();
        }

        [Fact]
        public void Should_trim_value()
        {
            var result = Guard.Optional("   value   ", "Field", 100);
            result.Should().Be("value");
        }

        [Fact]
        public void Should_throw_when_value_exceeds_max_length()
        {
            var act = () => Guard.Optional("a".PadRight(101, 'a'), "Field", 100);
            act.Should().Throw<DomainValidationException>()
                .Which.Code.Should().Be("FIELD_TOO_LONG");
        }
    }

    public class ErrorFactoryMethods
    {
        [Fact]
        public void RequiredError_should_return_DomainValidationException()
        {
            var error = Guard.RequiredError("Title", "BOOK");
            error.Code.Should().Be("BOOK_TITLE_REQUIRED");
            error.Errors.Should().ContainKey("Title");
        }

        [Fact]
        public void TooLongError_should_return_DomainValidationException()
        {
            var error = Guard.TooLongError("Title", 200, "BOOK");
            error.Code.Should().Be("BOOK_TITLE_TOO_LONG");
            error.Errors.Should().ContainKey("Title");
        }

        [Fact]
        public void BuildCode_without_prefix()
        {
            var act = () => Guard.Required(null, "Name", 100);
            act.Should().Throw<DomainValidationException>()
                .Which.Code.Should().Be("NAME_REQUIRED");
        }
    }
}
