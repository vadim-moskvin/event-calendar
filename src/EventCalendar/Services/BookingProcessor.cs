using EventCalendar.Models;

namespace EventCalendar.Services;

public class BookingProcessor : BackgroundService
{
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly IBookingStore _bookingStore;
    private readonly IEventStore _eventStore;
    private readonly ILogger<BookingProcessor> _logger;

    public BookingProcessor(IBookingStore bookingStore, IEventStore eventStore, ILogger<BookingProcessor> logger)
    {
        _bookingStore = bookingStore;
        _eventStore = eventStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var bookings = (await _bookingStore.GetPendingBookingsAsync()).ToList();
            var tasks = bookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);

        await _processingSemaphore.WaitAsync(stoppingToken);

        var @event = _eventStore.Get(booking.EventId);
        try
        {
            if (@event != null)
            {
                booking.Confirm();
            }
            else
            {
                booking.Reject();
                _logger.LogWarning("Event not found for booking {BookingId}", booking.Id);
            }
        }
        catch (Exception)
        {
            booking.Reject();
            if (@event != null)
            {
                @event.ReleaseSeats();
                _eventStore.Update(@event);
            }
        }
        finally
        {
            await _bookingStore.CreateOrUpdateBookingAsync(booking);
            _processingSemaphore.Release();
        }
    }
}