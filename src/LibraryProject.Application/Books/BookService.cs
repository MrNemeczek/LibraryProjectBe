using LibraryProject.Application.Books.Exceptions;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Books;

internal sealed class BookService(
    IBookRepository bookRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await bookRepository.CountAsync(cancellationToken);
        var books = await bookRepository.GetAsync(page, pageSize, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<BookResponse>(
            books.Select(MapToResponse).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<BookResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var book = await GetExistingBookAsync(id, cancellationToken);

        return MapToResponse(book);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var isbn = CreateIsbn(request.Isbn);
        var isbnExists = await bookRepository.ExistsByIsbnAsync(isbn, excludedBookId: null, cancellationToken);
        if (isbnExists)
        {
            throw new BookIsbnAlreadyExistsException(isbn.Value);
        }

        var category = await GetOrCreateCategoryAsync(request.CategoryName, cancellationToken);
        var book = ExecuteDomainOperation(() => Book.Create(
            request.Title,
            request.Author,
            isbn,
            request.Description,
            category));

        bookRepository.Add(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(book);
    }

    public async Task<BookResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var book = await GetExistingBookAsync(id, cancellationToken);
        var isbn = CreateIsbn(request.Isbn);
        var isbnExists = await bookRepository.ExistsByIsbnAsync(isbn, id, cancellationToken);
        if (isbnExists)
        {
            throw new BookIsbnAlreadyExistsException(isbn.Value);
        }

        var category = await GetOrCreateCategoryAsync(request.CategoryName, cancellationToken);

        ExecuteDomainOperation(() => book.UpdateDetails(
            request.Title,
            request.Author,
            isbn,
            request.Description,
            category));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(book);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var book = await GetExistingBookAsync(id, cancellationToken);

        book.Delete(DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Book> GetExistingBookAsync(int id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(id);
        }

        return book;
    }

    private async Task<Category> GetOrCreateCategoryAsync(string categoryName, CancellationToken cancellationToken)
    {
        var normalizedName = ExecuteDomainOperation(() => Category.NormalizeName(categoryName));
        var category = await categoryRepository.GetByNameAsync(normalizedName, cancellationToken);
        if (category is not null)
        {
            return category;
        }

        category = ExecuteDomainOperation(() => Category.Create(normalizedName));

        categoryRepository.Add(category);
        return category;
    }

    private static BookResponse MapToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn.Value,
            book.Description,
            book.CategoryId,
            book.Category.Name);
    }

    private static Isbn CreateIsbn(string value)
    {
        return ExecuteDomainOperation(() => Isbn.Create(value));
    }

    private static T ExecuteDomainOperation<T>(Func<T> operation)
    {
        try
        {
            return operation();
        }
        catch (DomainException exception)
        {
            throw new DomainRuleViolationException(exception);
        }
    }

    private static void ExecuteDomainOperation(Action operation)
    {
        try
        {
            operation();
        }
        catch (DomainException exception)
        {
            throw new DomainRuleViolationException(exception);
        }
    }
}
