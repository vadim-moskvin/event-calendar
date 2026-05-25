using EventCalendar.Exceptions;
using EventCalendar.Models;

namespace EventCalendar.Services;

public class BookingService(EventService eventService) : IBookingService
{
    private const string BookingNotFoundException = "Бронь не найдена";

    private readonly Dictionary<Guid, Booking> _bookings = [];

    public Guid CreateBookingAsync(Guid eventId)
    {
        _ = eventService.GetEvent(eventId);
        var booking = Booking.MakeNew(eventId);
        _bookings.Add(eventId, booking);
        return booking.Id;
    }

    public Booking GetBookingByIdAsync(Guid bookingId)
    {
        return _bookings.GetValueOrDefault(bookingId) ?? throw new NotFoundException(BookingNotFoundException);
    }
}