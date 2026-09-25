namespace EventCalendar.Bookings.Application.Services;

public interface IBookingProcessingService
{
    Task ProcessAsync(Guid bookingId, CancellationToken cancellationToken);
}