using LibraryProject.Application.Authorization;
using LibraryProject.Application.Books;
using LibraryProject.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers;

[Route("api/[controller]")]
public sealed class BooksController(IBookService bookService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<BookResponse>>> Get(
        [FromQuery] GetBooksRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await bookService.GetAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await bookService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var response = await bookService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookResponse>> Update(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var response = await bookService.UpdateAsync(id, request, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await bookService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
