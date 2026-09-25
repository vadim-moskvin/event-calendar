using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventCalendar.Bookings.Infrastructure.Services;

public sealed class BookingProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid[] ids;

            using (var scope = scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

                ids = (await repository.GetPendingBookingsAsync(stoppingToken))
                    .Select(x => x.Id)
                    .ToArray();
            }

            var tasks = ids.Select(id => ProcessAsync(id, stoppingToken));
            await Task.WhenAll(tasks);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessAsync(Guid id, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IBookingProcessingService>();

        try
        {
            await service.ProcessAsync(id, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process booking {BookingId}", id);
        }
    }
}
