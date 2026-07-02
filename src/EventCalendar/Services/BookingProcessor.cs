using EventCalendar.Models;

namespace EventCalendar.Services;

public class BookingProcessor : BackgroundService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly IBookingStore _store;

    public BookingProcessor(IEventService eventService, IBookingStore store)
    {
        _store = store;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var bookings = (await _store.GetPendingBookingsAsync()).ToList();
            var tasks = bookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);
        
        await _processingSemaphore.WaitAsync(stoppingToken);
        try
        {
            booking.Confirm();
            await _store.CreateOrUpdateBookingAsync(booking);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}