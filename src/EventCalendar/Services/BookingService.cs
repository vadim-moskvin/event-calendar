using EventCalendar.DataAccess;
using EventCalendar.Exceptions;
using EventCalendar.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Services;

public class BookingService(IEventService eventService, AppDbContext appDbContext) : IBookingService
{
    private const string BookingNotFoundException = "Бронь не найдена";

    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        await SemaphoreSlim.WaitAsync();

        try
        {
            var @event = await eventService.GetEventAsync(eventId);
            if (!@event.TryReserveSeats())
                throw new NoAvailableSeatsException();
            var booking = Booking.MakeNew(eventId);
            appDbContext.Bookings.Add(booking);
            await appDbContext.SaveChangesAsync();
        
            return booking;
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return await appDbContext.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId) ??
               throw new NotFoundException(BookingNotFoundException);
    }
}