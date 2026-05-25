using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IBookingService
{
    Guid CreateBookingAsync(Guid eventId);

    Booking GetBookingByIdAsync(Guid bookingId);
}