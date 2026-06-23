using EventCalendar.Repositories;

public class BookingProcessor(IBookingRepository repository) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var bookings = await repository.GetPendingBookingsAsync();

            foreach (var booking in bookings)
            {
                await Task.Delay(10000, stoppingToken);
                booking.Confirm();
                await repository.CreateOrUpdateBookingAsync(booking);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}