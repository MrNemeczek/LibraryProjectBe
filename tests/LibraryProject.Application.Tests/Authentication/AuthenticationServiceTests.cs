using AutoFixture;
using FluentAssertions;
using LibraryProject.Application.Authentication;
using LibraryProject.Application.Authentication.Exceptions;
using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.Enums;
using NSubstitute;

namespace LibraryProject.Application.Tests.Authentication;

public class AuthenticationServiceTests
{
    private readonly IFixture _fixture;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Reservation>(c => c.FromFactory(() => Reservation.Create(1, 1, 3)));
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordHasher = Substitute.For<IUserPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();

        _sut = new AuthenticationService(
            _userRepository, _unitOfWork, _passwordHasher, _jwtTokenGenerator);
    }

    public class RegisterAsync : AuthenticationServiceTests
    {
        [Fact]
        public async Task Should_create_user_and_return_authentication_response()
        {
            var request = _fixture.Build<RegisterRequest>()
                .With(r => r.Email, "Test@Example.com")
                .Create();
            var token = _fixture.Create<JwtToken>();

            _userRepository.ExistsByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
                .Returns(false);
            _passwordHasher.HashPassword(Arg.Any<User>(), request.Password)
                .Returns("hashed");
            _jwtTokenGenerator.GenerateToken(Arg.Any<User>())
                .Returns(token);

            var result = await _sut.RegisterAsync(request, CancellationToken.None);

            result.Token.Should().Be(token.Token);
            result.ExpiresAt.Should().Be(token.ExpiresAt);
            result.User.Email.Should().Be("test@example.com");
            result.User.Role.Should().Be(UserRole.Reader.ToString());

            _userRepository.Received(1).Add(Arg.Is<User>(u =>
                u.Email == "test@example.com" &&
                u.FirstName == request.FirstName.Trim() &&
                u.LastName == request.LastName.Trim() &&
                u.Role == UserRole.Reader &&
                u.IsActive));
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_email_already_exists()
        {
            var request = _fixture.Build<RegisterRequest>()
                .With(r => r.Email, "existing@example.com")
                .Create();

            _userRepository.ExistsByEmailAsync("existing@example.com", Arg.Any<CancellationToken>())
                .Returns(true);

            var act = () => _sut.RegisterAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<EmailAlreadyExistsException>()
                .Where(e => e.Code == "AUTH_EMAIL_ALREADY_EXISTS");
        }
    }

    public class LoginAsync : AuthenticationServiceTests
    {
        [Fact]
        public async Task Should_return_authentication_response_when_credentials_valid()
        {
            var request = _fixture.Build<LoginRequest>()
                .With(r => r.Email, "Test@Example.com")
                .Create();
            var user = _fixture.Build<User>()
                .With(u => u.Email, "test@example.com")
                .With(u => u.IsActive, true)
                .Create();
            var token = _fixture.Create<JwtToken>();

            _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
                .Returns(user);
            _passwordHasher.VerifyPassword(user, request.Password)
                .Returns(true);
            _jwtTokenGenerator.GenerateToken(user)
                .Returns(token);

            var result = await _sut.LoginAsync(request, CancellationToken.None);

            result.Token.Should().Be(token.Token);
            result.ExpiresAt.Should().Be(token.ExpiresAt);
        }

        [Fact]
        public async Task Should_throw_when_user_not_found()
        {
            var request = _fixture.Build<LoginRequest>()
                .With(r => r.Email, "unknown@example.com")
                .Create();

            _userRepository.GetByEmailAsync("unknown@example.com", Arg.Any<CancellationToken>())
                .Returns((User?)null);

            var act = () => _sut.LoginAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task Should_throw_when_user_is_inactive()
        {
            var request = _fixture.Build<LoginRequest>()
                .With(r => r.Email, "inactive@example.com")
                .Create();
            var user = _fixture.Build<User>()
                .With(u => u.Email, "inactive@example.com")
                .With(u => u.IsActive, false)
                .Create();

            _userRepository.GetByEmailAsync("inactive@example.com", Arg.Any<CancellationToken>())
                .Returns(user);

            var act = () => _sut.LoginAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task Should_throw_when_password_is_wrong()
        {
            var request = _fixture.Build<LoginRequest>()
                .With(r => r.Email, "test@example.com")
                .Create();
            var user = _fixture.Build<User>()
                .With(u => u.Email, "test@example.com")
                .With(u => u.IsActive, true)
                .Create();

            _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
                .Returns(user);
            _passwordHasher.VerifyPassword(user, request.Password)
                .Returns(false);

            var act = () => _sut.LoginAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }
}
