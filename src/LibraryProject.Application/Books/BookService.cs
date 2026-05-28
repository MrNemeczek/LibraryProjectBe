using LibraryProject.Application.Books.Exceptions;
using LibraryProject.Application.Common;
using LibraryProject.Application.Common.Exceptions;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Common;
using LibraryProject.Domain.Entities;
using LibraryProject.Domain.ValueObjects;

namespace LibraryProject.Application.Books;

internal sealed class BookService(
    IBookRepository bookRepository,
    IBookCopyRepository bookCopyRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IBookService
{
    public async Task<PaginatedResponse<BookResponse>> GetAsync(GetBooksRequest request, CancellationToken cancellationToken)
    {
        var totalCount = await bookRepository.CountAsync(request, cancellationToken);
        var books = await bookRepository.GetAsync(request, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PaginatedResponse<BookResponse>(
            books.Select(MapToResponse).ToList(), request.Page, request.PageSize, totalCount, totalPages);
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
            throw new BookIsbnAlreadyExistsException(isbn.Value);

        var category = await GetOrCreateCategoryAsync(request.CategoryName, cancellationToken);
        var book = DomainOperation.Execute(() => Book.Create(
            request.Title, request.Author, isbn, request.Description, category));

        bookRepository.Add(book);

        if (request.InventoryNumbers?.Count > 0)
        {
            var (validated, error) = ValidateInventoryNumbers(request.InventoryNumbers);
            if (error is not null)
                throw error;

            var existingNums = await bookCopyRepository.GetExistingInventoryNumbersAsync(validated!, cancellationToken);
            if (existingNums.Count > 0)
                throw new BookCopyInventoryNumberAlreadyExistsException(existingNums[0]);

            foreach (var inv in validated!)
            {
                DomainOperation.Execute(() => book.AddCopy(inv));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(book);
    }

    public async Task<BookResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var book = await GetExistingBookAsync(id, cancellationToken);
        var isbn = CreateIsbn(request.Isbn);
        var isbnExists = await bookRepository.ExistsByIsbnAsync(isbn, id, cancellationToken);
        if (isbnExists)
            throw new BookIsbnAlreadyExistsException(isbn.Value);

        var category = await GetOrCreateCategoryAsync(request.CategoryName, cancellationToken);
        DomainOperation.Execute(() => book.UpdateDetails(
            request.Title, request.Author, isbn, request.Description, category));

        if (request.InventoryNumbers?.Count > 0)
        {
            var (validated, error) = ValidateInventoryNumbers(request.InventoryNumbers);
            if (error is not null)
                throw error;

            var existingNumbers = book.Copies.Select(c => c.InventoryNumber).ToHashSet();
            var newNumbers = validated!.Where(n => !existingNumbers.Contains(n)).ToList();

            if (newNumbers.Count > 0)
            {
                var globalExistingNums = await bookCopyRepository.GetExistingInventoryNumbersAsync(newNumbers, cancellationToken);
                if (globalExistingNums.Count > 0)
                    throw new BookCopyInventoryNumberAlreadyExistsException(globalExistingNums[0]);

                foreach (var inv in newNumbers)
                {
                    DomainOperation.Execute(() => book.AddCopy(inv));
                }
            }
        }

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
            throw new BookNotFoundException(id);
        return book;
    }

    private async Task<Category> GetOrCreateCategoryAsync(string categoryName, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByNameAsync(categoryName, cancellationToken);
        if (category is not null)
            return category;

        category = DomainOperation.Execute(() => Category.Create(categoryName));
        categoryRepository.Add(category);
        return category;
    }

    private static (List<string>? Validated, Exception? Error) ValidateInventoryNumbers(List<string> inventoryNumbers)
    {
        var normalized = inventoryNumbers
            .Select(n => n?.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)
            .ToList();

        if (normalized.Count == 0)
        {
            var validationError = new DomainValidationException(
                "BOOKCOPY_INVENTORY_NUMBERS_EMPTY",
                "Inventory numbers are invalid.",
                nameof(inventoryNumbers),
                "At least one valid inventory number is required.");
            return (null, new DomainRuleViolationException(validationError));
        }

        var duplicates = normalized
            .GroupBy(n => n)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            var validationError = new DomainValidationException(
                "BOOKCOPY_DUPLICATE_INVENTORY_NUMBERS",
                $"Duplicate inventory numbers: {string.Join(", ", duplicates)}.",
                nameof(inventoryNumbers),
                "Inventory numbers within the request must be unique.");
            return (null, new DomainRuleViolationException(validationError));
        }

        return (normalized, null);
    }

    private static BookResponse MapToResponse(Book book)
    {
        var copies = book.Copies
            .Select(c => new BookCopyResponse(c.Id, c.InventoryNumber, c.Status.ToString()))
            .ToList();

        return new BookResponse(
            book.Id, book.Title, book.Author, book.Isbn.Value,
            book.Description, book.CategoryId, book.Category.Name, copies);
    }

    private static Isbn CreateIsbn(string value)
    {
        return DomainOperation.Execute(() => Isbn.Create(value));
    }
}
