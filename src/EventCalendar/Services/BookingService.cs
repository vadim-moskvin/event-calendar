using EventCalendar.Models;

namespace EventCalendar.Services;

public class BookingService
{
    private readonly Dictionary<Guid, Booking> _bookings = [];
}