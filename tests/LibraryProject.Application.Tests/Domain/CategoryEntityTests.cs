using FluentAssertions;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Tests.Domain;

public class CategoryEntityTests
{
    [Fact]
    public void Create_should_set_name()
    {
        var category = Category.Create("Fiction");
        category.Name.Should().Be("Fiction");
    }

    [Fact]
    public void Create_should_trim_name()
    {
        var category = Category.Create("  Fiction  ");
        category.Name.Should().Be("Fiction");
    }

    [Fact]
    public void Create_should_throw_when_name_is_null()
    {
        var act = () => Category.Create(null);
        act.Should().Throw<DomainValidationException>()
            .Which.Code.Should().Be("CATEGORY_NAME_REQUIRED");
    }
}
