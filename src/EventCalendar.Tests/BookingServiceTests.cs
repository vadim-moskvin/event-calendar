using EventCalendar.Exceptions;
using EventCalendar.Models;
using EventCalendar.Tests.TestHelpers;

namespace EventCalendar.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task Book_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 5;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        // Act
        var booking = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotNull(booking);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(4, @event.AvailableSeats);
    }

    [Fact]
    public async Task Book_all_seats()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 5;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        // Act
        var bookings = new List<Booking>();
        foreach (var seat in Enumerable.Range(1, seats))
        {
            var booking = await bookingService.CreateBookingAsync(eventId);
            bookings.Add(booking);
        }

        // Assert
        Assert.DoesNotContain(bookings, booking => booking == null);
        Assert.DoesNotContain(bookings, booking => booking.Id == Guid.Empty);
        Assert.DoesNotContain(bookings, booking => booking.EventId != eventId);
        Assert.Equal(5, bookings.GroupBy(x => x.Id).Count());
        Assert.Equal(0, @event.AvailableSeats);

        // Act + Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(async () =>
            await bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Multiply_book_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(id: eventId);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        // Act
        var booking1 = await bookingService.CreateBookingAsync(eventId);
        var booking2 = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
    }

    [Fact]
    public async Task Get_booking()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(id: eventId);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);
        var newBooking = await bookingService.CreateBookingAsync(eventId);

        // Act
        var booking = await bookingService.GetBookingByIdAsync(newBooking.Id);

        // Assert
        Assert.Equal(newBooking.Id, booking.Id);
        Assert.Equal(newBooking.EventId, booking.EventId);
    }

    [Fact]
    public async Task Book_non_existing_event()
    {
        // Arrange
        var eventService = TestServiceFactory.MakeEventService();
        var eventId = Guid.NewGuid();

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Book_deleted_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(id: eventId);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);
        eventService.RemoveEventAsync(eventId);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Get_non_existing_booking()
    {
        // Arrange
        var eventService = TestServiceFactory.MakeEventService();
        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await bookingService.GetBookingByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Overbook()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 5;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        const int requestCount = 20;

        // Act
        var tasks = new List<Task<Booking>>();
        for (var i = 0; i < requestCount; i++)
            tasks.Add(Task.Run(() => bookingService.CreateBookingAsync(eventId)));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (NoAvailableSeatsException)
        {
        }

        // Assert
        Assert.Equal(5, tasks.Where(x => !x.IsFaulted)
            .Select(x => x.Result)
            .Count(x => x.Status == BookingStatus.Pending));

        Assert.Equal(15,
            tasks.Count(x =>
                x.IsFaulted && x.Exception?.InnerExceptions.Any(e => e is NoAvailableSeatsException) == true));

        Assert.Equal(0, @event.AvailableSeats);
    }

    [Fact]
    public async Task Book_parallel()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 10;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);

        const int requestCount = 10;

        // Act
        var tasks = new List<Task<Booking>>();
        for (var i = 0; i < requestCount; i++)
            tasks.Add(Task.Run(() => bookingService.CreateBookingAsync(eventId)));

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, tasks.GroupBy(x => x.Result.Id).Count());
    }

    [Fact]
    public async Task Reject_booking()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 1;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);

        var eventService = TestServiceFactory.MakeEventService();
        eventService.AddEventAsync(@event);

        var bookingService = TestServiceFactory.MakeBookingService(eventService);
        var booking = await bookingService.CreateBookingAsync(eventId);

        // Act
        booking.Reject();
        @event.ReleaseSeats();

        // Assert
        Assert.Equal(seats, @event.AvailableSeats);

        // Act + Assert
        await bookingService.CreateBookingAsync(eventId);
    }
}