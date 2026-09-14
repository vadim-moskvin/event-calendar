using System.Collections.Concurrent;
using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Exceptions;
using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public class BookingService(IEventService eventService, IBookingRepository bookingRepository) : IBookingService
{
    private const string BookingNotFoundException = "Бронь не найдена";

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> EventLocks = new();

    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        var semaphore = EventLocks.GetOrAdd(eventId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

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
            semaphore.Release();
        }
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return await bookingRepository.GetBookingAsync(bookingId) ??
               throw new NotFoundException(BookingNotFoundException);
    }
}