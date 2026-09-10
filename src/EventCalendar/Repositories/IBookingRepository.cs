using EventCalendar.Models;

namespace EventCalendar.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetBookingAsync(Guid id);

    Task<Booking> CreateBookingAsync(Booking bookingToCreate);
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}