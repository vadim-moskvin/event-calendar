using EventCalendar.Models;

namespace EventCalendar.Repositories;

public interface IBookingRepository
{
    Task CreateOrUpdateBookingAsync(Booking booking);

    Task<Booking?> GetBookingByIdAsync(Guid id);

    Task<IEnumerable<Booking>> GetPendingBookingsAsync();
}