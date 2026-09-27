using EventCalendar.Bookings.Domain.Models;

namespace EventCalendar.Bookings.Application.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid userId, Guid eventId);
    
    Task CancelBookingAsync(Guid bookingId, Guid userId, bool isAdmin);

    Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, bool isAdmin);
}
