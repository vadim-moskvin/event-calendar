using EventCalendar.DataAccess;
using EventCalendar.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Services;

public class BookingProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<BookingProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var bookings = await context.Bookings.Where(x => x.Status == BookingStatus.Pending).Select(x => x.Id)
                .ToArrayAsync(cancellationToken: stoppingToken);
            var tasks = bookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await Task.Delay(2000, stoppingToken);

        var booking = await context.Bookings.Include(x => x.Event)
            .FirstAsync(x => x.Id == bookingId, stoppingToken);
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
            context.Bookings.Update(booking);
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}