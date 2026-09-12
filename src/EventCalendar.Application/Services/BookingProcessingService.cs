using EventCalendar.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace EventCalendar.Application.Services;

public sealed class BookingProcessingService(
    IBookingRepository bookingRepository,
    ILogger<BookingProcessingService> logger)
    : IBookingProcessingService
{
    public async Task ProcessAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var booking = await bookingRepository.GetBookingAsync(bookingId);
        if (booking is null)
            return;

        try
        {
            if (booking.Event is not null)
            {
                booking.Confirm();
            }
            else
            {
                booking.Reject();
                logger.LogWarning("Event not found for booking {BookingId}", booking.Id);
            }
        }
        catch (Exception)
        {
            booking.Reject();
            booking.Event?.ReleaseSeats();
        }

        await bookingRepository.SaveChangesAsync(cancellationToken);
    }
}