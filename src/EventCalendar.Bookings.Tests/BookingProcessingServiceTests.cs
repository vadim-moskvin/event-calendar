using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Application.Services;
using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Bookings.Tests.TestHelpers;
using EventCalendar.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.Tests;

public class BookingProcessingServiceTests : TestsBase
{
    [Fact]
    public async Task Process_pending_booking()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var booking = await BookingService.CreateBookingAsync(userId, eventId);
        var repository = ServiceProvider.GetRequiredService<IBookingRepository>();
        BookingConfirmed? published = null;
        BookingStatus? statusAtPublish = null;
        var publisher = new TestPublisher(async message =>
        {
            using var scope = ServiceProvider.CreateScope();
            var stored = await scope.ServiceProvider.GetRequiredService<IBookingRepository>()
                .GetBookingAsync(booking.Id);
            statusAtPublish = stored?.Status;
            published = message;
        });
        var processor = new BookingProcessingService(repository, publisher);

        // Act
        await processor.ProcessAsync(booking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, statusAtPublish);
        Assert.NotNull(published);
        Assert.Equal(booking.Id, published.BookingId);
        Assert.Equal(eventId, published.EventId);
        Assert.Equal(userId, published.UserId);
        Assert.Equal(1, published.SeatCount);
        Assert.Equal(booking.ProcessedAt, published.ConfirmedAt);
        Assert.Equal(DateTimeKind.Utc, published.ConfirmedAt.Kind);
    }

    [Fact]
    public async Task Process_cancelled_booking()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var booking = await BookingService.CreateBookingAsync(userId, Guid.NewGuid());
        await BookingService.CancelBookingAsync(booking.Id, userId, false);
        var publisher = new TestPublisher(_ => throw new Xunit.Sdk.XunitException("Unexpected publication"));
        var processor = new BookingProcessingService(
            ServiceProvider.GetRequiredService<IBookingRepository>(), publisher);

        // Act
        await processor.ProcessAsync(booking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    private sealed class TestPublisher(Func<BookingConfirmed, Task> publish) : IBookingConfirmedPublisher
    {
        public Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default)
            => publish(message);
    }
}
