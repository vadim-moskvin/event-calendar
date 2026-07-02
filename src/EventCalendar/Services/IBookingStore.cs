using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IBookingStore
{
    Task CreateOrUpdateBookingAsync(Booking booking);

    Task<Booking?> GetBookingByIdAsync(Guid id);

    Task<IEnumerable<Booking>> GetPendingBookingsAsync();
}