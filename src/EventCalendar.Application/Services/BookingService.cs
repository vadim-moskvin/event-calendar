using System.Collections.Concurrent;
using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Exceptions;
using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public class BookingService(IEventService eventService, IBookingRepository bookingRepository) : IBookingService
{
    private const int MaxBookingPerUser = 10;

    private const string BookingNotFoundException = "Бронь не найдена";

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> EventLocks = new();

    public async Task<Booking> CreateBookingAsync(Guid userId, Guid eventId)
    {
        var semaphore = EventLocks.GetOrAdd(eventId, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

        try
        {
            var @event = await eventService.GetEventAsync(eventId);

            if (@event.StartAt > DateTime.UtcNow)
                throw new EventAlreadyStartedException();
            var bookingCount = await bookingRepository
                .GetActiveBookingCountByUserIdAsync(userId);
            if (bookingCount >= MaxBookingPerUser)
                throw new MaxBookingPerUserException();
            if (!@event.TryReserveSeats())
                throw new NoAvailableSeatsException();

            var booking = Booking.MakeNew(userId, eventId);
            await bookingRepository.CreateBookingAsync(booking);
            await bookingRepository.SaveChangesAsync();

            return booking;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid userId, Role actorRole)
    {
        var booking = await bookingRepository.GetBookingAsync(bookingId)
                      ?? throw new NotFoundException(BookingNotFoundException);

        var isOwner = booking.UserId == userId;
        var isAdmin = actorRole == Role.Admin;

        if (!isOwner && !isAdmin)
            throw new NotAllowedException();

        booking.Cancel();
        await bookingRepository.SaveChangesAsync();
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return await bookingRepository.GetBookingAsync(bookingId) ??
               throw new NotFoundException(BookingNotFoundException);
    }
}