using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Exceptions;
using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public class BookingService(IEventService eventService, IBookingRepository bookingRepository) : IBookingService
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
            await bookingRepository.CreateBookingAsync(booking);
            await bookingRepository.SaveChangesAsync();

            return booking;
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return await bookingRepository.GetBookingAsync(bookingId) ??
               throw new NotFoundException(BookingNotFoundException);
    }
}