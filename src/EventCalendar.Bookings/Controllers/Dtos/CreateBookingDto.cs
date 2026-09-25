namespace EventCalendar.Bookings.Controllers.Dtos;

public sealed record CreateBookingDto
{
    public required Guid EventId { get; init; }
}
