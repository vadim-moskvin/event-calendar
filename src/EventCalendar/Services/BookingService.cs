using EventCalendar.Exceptions;
using EventCalendar.Models;

namespace EventCalendar.Services;

public class BookingService(IEventService eventService, IBookingStore bookingStore) : IBookingService
{
    private const string BookingNotFoundException = "Бронь не найдена";

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        await _semaphoreSlim.WaitAsync();

        var @event = eventService.GetEvent(eventId);
        if (!@event.TryReserveSeats())
            throw new NoAvailableSeatsException();
        var booking = Booking.MakeNew(eventId);
        await bookingStore.CreateOrUpdateBookingAsync(booking);

        _semaphoreSlim.Release();
        return booking;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return await bookingStore.GetBookingByIdAsync(bookingId) ??
               throw new NotFoundException(BookingNotFoundException);
    }
}