using EventCalendar.Domain.Models;

namespace EventCalendar.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId);

    Task<Booking> GetBookingByIdAsync(Guid bookingId);
}