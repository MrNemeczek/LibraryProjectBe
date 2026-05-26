using LibraryProject.Application.Authorization;
using LibraryProject.Application.Common.Pagination;
using LibraryProject.Application.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers;

[Route("api/[controller]")]
public sealed class ReservationsController(IReservationService reservationService) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ReservationResponse>>> GetMyReservations(
        [FromQuery] GetReservationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var response = await reservationService.GetMyReservationsAsync(userId, request.Page, request.PageSize, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var response = await reservationService.GetByIdAsync(id, userId, role, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpGet("all")]
    public async Task<ActionResult<PaginatedResponse<ReservationResponse>>> GetAll(
        [FromQuery] GetReservationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await reservationService.GetAllAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var response = await reservationService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await reservationService.CancelAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = ApplicationRoles.LibrarianOrAdministrator)]
    [HttpPut("{id:int}/fulfill")]
    public async Task<IActionResult> Fulfill(int id, CancellationToken cancellationToken)
    {
        await reservationService.FulfillAsync(id, cancellationToken);
        return NoContent();
    }

}
