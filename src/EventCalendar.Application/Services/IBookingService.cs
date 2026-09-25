using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid userId, Guid eventId);
    
    Task CancelBookingAsync(Guid bookingId, Guid userId, Role actorRole);

    Task<Booking> GetBookingByIdAsync(Guid bookingId);
}