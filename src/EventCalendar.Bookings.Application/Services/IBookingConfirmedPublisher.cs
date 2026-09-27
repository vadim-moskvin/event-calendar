using EventCalendar.Contracts;

namespace EventCalendar.Bookings.Application.Services;

public interface IBookingConfirmedPublisher
{
    Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default);
}
