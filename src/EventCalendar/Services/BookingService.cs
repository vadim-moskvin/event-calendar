using EventCalendar.Exceptions;
using EventCalendar.Models;

namespace EventCalendar.Services;

public class BookingService(IEventService eventService) : IBookingService
{
    private const string BookingNotFoundException = "Бронь не найдена";

    private readonly Dictionary<Guid, Booking> _bookings = [];

    public Task<Booking> CreateBookingAsync(Guid eventId)
    {
        _ = eventService.GetEvent(eventId);
        var booking = Booking.MakeNew(eventId);
        _bookings.Add(booking.Id, booking);
        return Task.FromResult(booking);
    }

    public Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return Task.FromResult(_bookings.GetValueOrDefault(bookingId) ??
                               throw new NotFoundException(BookingNotFoundException));
    }
}