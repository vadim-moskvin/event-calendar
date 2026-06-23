using EventCalendar.Models;

namespace EventCalendar.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly Dictionary<Guid, Booking> _bookings = [];

    public Task CreateOrUpdateBookingAsync(Booking booking)
    {
        _bookings[booking.Id] = booking;
        return Task.CompletedTask;
    }

    public Task<Booking?> GetBookingByIdAsync(Guid id)
    {
        return Task.FromResult(_bookings.GetValueOrDefault(id));
    }

    public Task<IEnumerable<Booking>> GetPendingBookingsAsync()
    {
        return Task.FromResult(_bookings.Values.Where(x => x.Status == BookingStatus.Pending));
    }
}