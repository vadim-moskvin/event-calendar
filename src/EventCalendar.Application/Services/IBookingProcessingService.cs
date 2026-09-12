namespace EventCalendar.Application.Services;

public interface IBookingProcessingService
{
    Task ProcessAsync(Guid bookingId, CancellationToken cancellationToken);
}