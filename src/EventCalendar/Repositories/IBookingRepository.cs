using EventCalendar.Models;

namespace EventCalendar.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetBookingAsync(Guid id);
    
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct = default);

    Task<Booking> CreateBookingAsync(Booking bookingToCreate);
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}