using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Bookings.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext appDbContext) : IBookingRepository
{
    public async Task<Booking?> GetBookingAsync(Guid id)
    {
        return await appDbContext.Bookings.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct = default)
    {
        return await appDbContext.Bookings
            .Where(x => x.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<Booking> CreateBookingAsync(Booking bookingToCreate)
    {
        await appDbContext.Bookings.AddAsync(bookingToCreate);
        return bookingToCreate;
    }
    
    public Task<int> GetActiveBookingCountByUserIdAsync(Guid userId)
    {
        return appDbContext.Bookings.CountAsync(x =>
            x.UserId == userId &&
            (x.Status == BookingStatus.Pending ||
             x.Status == BookingStatus.Confirmed));
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }
}
