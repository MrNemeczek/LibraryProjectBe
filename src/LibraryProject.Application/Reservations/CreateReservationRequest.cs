using System.ComponentModel.DataAnnotations;

namespace LibraryProject.Application.Reservations;

public sealed record CreateReservationRequest(
    [Required] int BookId,
    [Range(1, 7)] int? PickupDeadlineDays = null);
