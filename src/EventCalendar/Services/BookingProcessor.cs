using EventCalendar.Repositories;

namespace EventCalendar.Services;

public class BookingProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<BookingProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var bookings = (await bookingRepository.GetPendingBookingsAsync(stoppingToken))
                .Select(x => x.Id)
                .ToArray();
            var tasks = bookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        await Task.Delay(2000, stoppingToken);

        var booking = await bookingRepository.GetBookingAsync(bookingId);
        try
        {
            if (booking.Event != null)
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
        finally
        {
            await bookingRepository.SaveChangesAsync(stoppingToken);
        }
    }
}