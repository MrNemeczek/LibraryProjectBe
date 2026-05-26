using LibraryProject.Application.Authorization;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers;

[Route("api/[controller]")]
public sealed class LoansController(ILoanService loanService) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<LoanResponse>>> GetMyLoans(
        [FromQuery] GetLoansRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var response = await loanService.GetMyLoansAsync(userId, request.Page, request.PageSize, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpGet("all")]
    public async Task<ActionResult<PaginatedResponse<LoanResponse>>> GetAll(
        [FromQuery] GetLoansRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await loanService.GetAllAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var response = await loanService.GetByIdAsync(id, userId, role, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> Return(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        await loanService.ReturnAsync(id, userId, role, cancellationToken);
        return NoContent();
    }
}
