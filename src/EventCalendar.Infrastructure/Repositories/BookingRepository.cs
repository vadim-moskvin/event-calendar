using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Models;
using EventCalendar.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Infrastructure.Repositories;

public class BookingRepository(AppDbContext appDbContext) : IBookingRepository
{
    public async Task<Booking?> GetBookingAsync(Guid id)
    {
        return await appDbContext.Bookings
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct = default)
    {
        return await appDbContext.Bookings
            .Include(x => x.Event)
            .Where(x => x.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<Booking> CreateBookingAsync(Booking bookingToCreate)
    {
        await appDbContext.Bookings.AddAsync(bookingToCreate);
        return bookingToCreate;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }
}