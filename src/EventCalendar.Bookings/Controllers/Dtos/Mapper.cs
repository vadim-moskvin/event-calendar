using EventCalendar.Bookings.Domain.Models;

namespace EventCalendar.Bookings.Controllers.Dtos;

public static class Mapper
{
    public static BookingDto ToDto(this Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
    }
}
