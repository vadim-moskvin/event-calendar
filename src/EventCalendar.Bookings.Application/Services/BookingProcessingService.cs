using EventCalendar.Bookings.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace EventCalendar.Bookings.Application.Services;

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

        // Здесь будет отправка запроса на резервирование места через Kafka.
        // До получения ответа Events бронь должна оставаться в статусе Pending.
        logger.LogDebug("Booking {BookingId} is waiting for event processing", booking.Id);
    }
}
