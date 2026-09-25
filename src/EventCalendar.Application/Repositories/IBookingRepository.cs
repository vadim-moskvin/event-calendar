using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetBookingAsync(Guid id);

    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct = default);

    Task<Booking> CreateBookingAsync(Booking bookingToCreate);
    
    Task<int> GetActiveBookingCountByUserIdAsync(Guid userId);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}