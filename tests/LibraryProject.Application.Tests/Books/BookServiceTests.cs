using AutoFixture;
using FluentAssertions;
using LibraryProject.Application.Books;
using LibraryProject.Application.Books.Exceptions;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;
using NSubstitute;

namespace LibraryProject.Application.Tests.Books;

public class BookServiceTests
{
    protected readonly IFixture _fixture;
    protected readonly IBookRepository _bookRepository;
    protected readonly IBookCopyRepository _bookCopyRepository;
    protected readonly ICategoryRepository _categoryRepository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IBookService _sut;

    public BookServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Isbn>(c => c.FromFactory(() => Isbn.Create("978-3-16-148410-0")));
        _fixture.Customize<Category>(c => c.FromFactory(() => Category.Create("Fiction")));
        _fixture.Customize<Book>(c => c.FromFactory(() =>
        {
            var book = Book.Create("Title", "Author", Isbn.Create("978-3-16-148410-0"), null, Category.Create("Fiction"));
            typeof(Book).GetProperty("Id")!.SetValue(book, 1);
            return book;
        }));
        _fixture.Customize<CreateBookRequest>(c => c
            .With(r => r.Isbn, "978-3-16-148410-0")
            .With(r => r.CategoryName, "Fiction"));
        _fixture.Customize<UpdateBookRequest>(c => c
            .With(r => r.Isbn, "978-3-16-148410-0")
            .With(r => r.CategoryName, "Fiction"));

        _bookRepository = Substitute.For<IBookRepository>();
        _bookCopyRepository = Substitute.For<IBookCopyRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _sut = new BookService(_bookRepository, _bookCopyRepository, _categoryRepository, _unitOfWork);
    }

    public class GetAsync : BookServiceTests
    {
        [Fact]
        public async Task Should_return_paginated_books()
        {
            var books = new List<Book> { _fixture.Create<Book>() };
            var request = new GetBooksRequest { Page = 1, PageSize = 20 };
            _bookRepository.CountAsync(request, Arg.Any<CancellationToken>()).Returns(1);
            _bookRepository.GetAsync(request, Arg.Any<CancellationToken>()).Returns(books);

            var result = await _sut.GetAsync(request, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.TotalPages.Should().Be(1);
            result.Page.Should().Be(1);
        }

        [Fact]
        public async Task Should_return_empty_when_no_books()
        {
            var request = new GetBooksRequest { Page = 1, PageSize = 20 };
            _bookRepository.CountAsync(request, Arg.Any<CancellationToken>()).Returns(0);
            _bookRepository.GetAsync(request, Arg.Any<CancellationToken>())
                .Returns(new List<Book>());

            var result = await _sut.GetAsync(request, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task Should_pass_filters_to_repository()
        {
            var request = new GetBooksRequest
            {
                Page = 2,
                PageSize = 10,
                Title = "Dune",
                Author = "Herbert",
                CategoryId = 1,
            };
            _bookRepository.CountAsync(request, Arg.Any<CancellationToken>()).Returns(11);
            _bookRepository.GetAsync(request, Arg.Any<CancellationToken>())
                .Returns(new List<Book>());

            var result = await _sut.GetAsync(request, CancellationToken.None);

            result.Page.Should().Be(2);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(11);
            result.TotalPages.Should().Be(2);
            await _bookRepository.Received(1).CountAsync(request, Arg.Any<CancellationToken>());
            await _bookRepository.Received(1).GetAsync(request, Arg.Any<CancellationToken>());
        }
    }

    public class GetByIdAsync : BookServiceTests
    {
        [Fact]
        public async Task Should_return_book_when_found()
        {
            var book = _fixture.Create<Book>();
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(book);

            var result = await _sut.GetByIdAsync(1, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_throw_when_book_not_found()
        {
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Book?)null);

            var act = () => _sut.GetByIdAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<BookNotFoundException>();
        }
    }

    public class CreateAsync : BookServiceTests
    {
        [Fact]
        public async Task Should_create_book_without_copies()
        {
            var request = _fixture.Build<CreateBookRequest>()
                .With(r => r.Isbn, "978-3-16-148410-0")
                .With(r => r.CategoryName, "Fiction")
                .With(r => r.InventoryNumbers, (List<string>?)null)
                .Create();
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), null, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((Category?)null);

            var result = await _sut.CreateAsync(request, CancellationToken.None);

            result.Should().NotBeNull();
            _bookRepository.Received(1).Add(Arg.Any<Book>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_create_book_with_copies()
        {
            var request = _fixture.Build<CreateBookRequest>()
                .With(r => r.Isbn, "978-3-16-148410-0")
                .With(r => r.CategoryName, "Fiction")
                .With(r => r.InventoryNumbers, new List<string> { "INV-001" })
                .Create();
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), null, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((Category?)null);
            _bookCopyRepository.GetExistingInventoryNumbersAsync(
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new List<string>());

            var result = await _sut.CreateAsync(request, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_throw_when_isbn_already_exists()
        {
            var request = _fixture.Create<CreateBookRequest>();
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), null, Arg.Any<CancellationToken>())
                .Returns(true);

            var act = () => _sut.CreateAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<BookIsbnAlreadyExistsException>();
        }

        [Fact]
        public async Task Should_throw_when_inventory_numbers_are_empty()
        {
            var request = _fixture.Build<CreateBookRequest>()
                .With(r => r.Isbn, "978-3-16-148410-0")
                .With(r => r.CategoryName, "Fiction")
                .With(r => r.InventoryNumbers, new List<string> { "" })
                .Create();
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), null, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Category.Create("Fiction"));

            var act = () => _sut.CreateAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<DomainRuleViolationException>()
                .Where(e => e.Code == "BOOKCOPY_INVENTORY_NUMBERS_EMPTY");
        }

        [Fact]
        public async Task Should_throw_when_inventory_numbers_have_duplicates()
        {
            var request = _fixture.Build<CreateBookRequest>()
                .With(r => r.Isbn, "978-3-16-148410-0")
                .With(r => r.CategoryName, "Fiction")
                .With(r => r.InventoryNumbers, new List<string> { "INV-001", "INV-001" })
                .Create();
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), null, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Category.Create("Fiction"));

            var act = () => _sut.CreateAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<DomainRuleViolationException>()
                .Where(e => e.Code == "BOOKCOPY_DUPLICATE_INVENTORY_NUMBERS");
        }

        [Fact]
        public async Task Should_throw_when_inventory_numbers_already_exist_globally()
        {
            var request = _fixture.Build<CreateBookRequest>()
                .With(r => r.Isbn, "978-3-16-148410-0")
                .With(r => r.CategoryName, "Fiction")
                .With(r => r.InventoryNumbers, new List<string> { "INV-001" })
                .Create();
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), null, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Category.Create("Fiction"));
            _bookCopyRepository.GetExistingInventoryNumbersAsync(
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new List<string> { "INV-001" });

            var act = () => _sut.CreateAsync(request, CancellationToken.None);

            await act.Should().ThrowAsync<BookCopyInventoryNumberAlreadyExistsException>();
        }
    }

    public class UpdateAsync : BookServiceTests
    {
        [Fact]
        public async Task Should_update_book()
        {
            var book = _fixture.Create<Book>();
            var request = _fixture.Create<UpdateBookRequest>();
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(book);
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), 1, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Category.Create("Fiction"));

            var result = await _sut.UpdateAsync(1, request, CancellationToken.None);

            result.Should().NotBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_book_not_found()
        {
            var request = _fixture.Create<UpdateBookRequest>();
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Book?)null);

            var act = () => _sut.UpdateAsync(1, request, CancellationToken.None);

            await act.Should().ThrowAsync<BookNotFoundException>();
        }

        [Fact]
        public async Task Should_throw_when_isbn_already_exists_for_another_book()
        {
            var book = _fixture.Create<Book>();
            var request = _fixture.Create<UpdateBookRequest>();
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(book);
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), 1, Arg.Any<CancellationToken>())
                .Returns(true);

            var act = () => _sut.UpdateAsync(1, request, CancellationToken.None);

            await act.Should().ThrowAsync<BookIsbnAlreadyExistsException>();
        }

        [Fact]
        public async Task Should_throw_when_inventory_numbers_duplicate_globally()
        {
            var book = _fixture.Create<Book>();
            var request = _fixture.Build<UpdateBookRequest>()
                .With(r => r.Isbn, "978-3-16-148410-0")
                .With(r => r.CategoryName, "Fiction")
                .With(r => r.InventoryNumbers, new List<string> { "INV-999" })
                .Create();
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(book);
            _bookRepository.ExistsByIsbnAsync(Arg.Any<Isbn>(), 1, Arg.Any<CancellationToken>())
                .Returns(false);
            _categoryRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Category.Create("Fiction"));
            _bookCopyRepository.GetExistingInventoryNumbersAsync(
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new List<string> { "INV-999" });

            var act = () => _sut.UpdateAsync(1, request, CancellationToken.None);

            await act.Should().ThrowAsync<BookCopyInventoryNumberAlreadyExistsException>();
        }
    }

    public class DeleteAsync : BookServiceTests
    {
        [Fact]
        public async Task Should_soft_delete_book()
        {
            var book = _fixture.Create<Book>();
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(book);

            await _sut.DeleteAsync(1, CancellationToken.None);

            book.IsDeleted.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_throw_when_book_not_found()
        {
            _bookRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Book?)null);

            var act = () => _sut.DeleteAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<BookNotFoundException>();
        }
    }
}
