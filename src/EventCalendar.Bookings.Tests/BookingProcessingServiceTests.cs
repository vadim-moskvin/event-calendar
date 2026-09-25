using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Application.Services;
using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Bookings.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventCalendar.Bookings.Tests;

public class BookingProcessingServiceTests : TestsBase
{
    [Fact]
    public async Task Booking_stays_pending_until_event_processing_is_connected()
    {
        var booking = await BookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());
        var repository = ServiceProvider.GetRequiredService<IBookingRepository>();
        var processor = new BookingProcessingService(repository,
            NullLogger<BookingProcessingService>.Instance);

        await processor.ProcessAsync(booking.Id, CancellationToken.None);

        var saved = await repository.GetBookingAsync(booking.Id);
        Assert.Equal(BookingStatus.Pending, saved?.Status);
    }
}
