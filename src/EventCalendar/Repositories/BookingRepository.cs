using EventCalendar.DataAccess;
using EventCalendar.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Repositories;

public class BookingRepository(AppDbContext appDbContext) : IBookingRepository
{
    public async Task<Booking?> GetBookingAsync(Guid id)
    {
        return await appDbContext.Bookings.FirstOrDefaultAsync(x => x.Id == id);
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