using FluentAssertions;
using LibraryProject.Application.Common;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Domain.Common;

namespace LibraryProject.Application.Tests.Common;

public class DomainOperationTests
{
    [Fact]
    public void Execute_with_func_should_return_result_when_no_exception()
    {
        var result = DomainOperation.Execute(() => 42);
        result.Should().Be(42);
    }

    [Fact]
    public void Execute_with_action_should_not_throw_when_no_exception()
    {
        var executed = false;
        DomainOperation.Execute(() => { executed = true; });
        executed.Should().BeTrue();
    }

    [Fact]
    public void Execute_with_func_should_wrap_DomainException_into_DomainRuleViolationException()
    {
        var act = () => DomainOperation.Execute<int>(() =>
            throw new DomainValidationException("TEST_CODE", "test message", "field", "error"));

        act.Should().Throw<DomainRuleViolationException>()
            .Which.Code.Should().Be("TEST_CODE");
    }

    [Fact]
    public void Execute_with_action_should_wrap_DomainException_into_DomainRuleViolationException()
    {
        var act = () => DomainOperation.Execute(() =>
            throw new DomainValidationException("TEST_CODE", "test message", "field", "error"));

        act.Should().Throw<DomainRuleViolationException>()
            .Which.Code.Should().Be("TEST_CODE");
    }

    [Fact]
    public void Execute_with_func_should_not_wrap_non_domain_exceptions()
    {
        var act = () => DomainOperation.Execute<int>(() =>
            throw new InvalidOperationException("not domain"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_with_action_should_not_wrap_non_domain_exceptions()
    {
        var act = () => DomainOperation.Execute(() =>
            throw new InvalidOperationException("not domain"));

        act.Should().Throw<InvalidOperationException>();
    }
}
