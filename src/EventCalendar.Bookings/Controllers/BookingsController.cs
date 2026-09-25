using EventCalendar.Bookings.Application.Services;
using EventCalendar.Bookings.Controllers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Bookings.Controllers;

[ApiController]
[Route("bookings")]
[Authorize]
public sealed class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (request.EventId == Guid.Empty)
            return BadRequest("EventId не может быть пустым.");

        var booking = await bookingService.CreateBookingAsync(userId, request.EventId);
        return AcceptedAtAction(nameof(Get), new { id = booking.Id }, booking.ToDto());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> Get(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var booking = await bookingService.GetBookingByIdAsync(id, userId, User.IsInRole("Admin"));
        return Ok(booking.ToDto());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        await bookingService.CancelBookingAsync(id, userId, User.IsInRole("Admin"));
        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}
