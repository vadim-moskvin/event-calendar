using EventCalendar.Exceptions;
using EventCalendar.Models;
using EventCalendar.Tests.TestHelpers;

namespace EventCalendar.Tests;

public class BookingServiceTests : TestsBase
{
    [Fact]
    public async Task Book_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 5;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);
        
        await EventService.AddEventAsync(@event);

        // Act
        var booking = await BookingService.CreateBookingAsync(eventId);

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
        
        await EventService.AddEventAsync(@event);

        // Act
        var bookings = new List<Booking>();
        foreach (var seat in Enumerable.Range(1, seats))
        {
            var booking = await BookingService.CreateBookingAsync(eventId);
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
            await BookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Multiply_book_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(id: eventId);
        
        await EventService.AddEventAsync(@event);

        // Act
        var booking1 = await BookingService.CreateBookingAsync(eventId);
        var booking2 = await BookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
    }

    [Fact]
    public async Task Get_booking()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(id: eventId);
        
        await EventService.AddEventAsync(@event);
        
        var newBooking = await BookingService.CreateBookingAsync(eventId);

        // Act
        var booking = await BookingService.GetBookingByIdAsync(newBooking.Id);

        // Assert
        Assert.Equal(newBooking.Id, booking.Id);
        Assert.Equal(newBooking.EventId, booking.EventId);
    }

    [Fact]
    public async Task Book_non_existing_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await BookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Book_deleted_event()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(id: eventId);
        
        await EventService.AddEventAsync(@event);
        await EventService.RemoveEventAsync(eventId);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await BookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Get_non_existing_booking()
    {
        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await BookingService.GetBookingByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Overbook()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        const int seats = 5;
        var @event = TestServiceFactory.MakeEvent(id: eventId, totalSeats: seats);
        
        await EventService.AddEventAsync(@event);

        const int requestCount = 20;

        // Act
        var tasks = new List<Task<Booking>>();
        for (var i = 0; i < requestCount; i++)
            tasks.Add(Task.Run(() => BookingService.CreateBookingAsync(eventId)));

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
        
        await EventService.AddEventAsync(@event);

        const int requestCount = 10;

        // Act
        var tasks = new List<Task<Booking>>();
        for (var i = 0; i < requestCount; i++)
            tasks.Add(Task.Run(() => BookingService.CreateBookingAsync(eventId)));

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
        
        await EventService.AddEventAsync(@event);
        
        var booking = await BookingService.CreateBookingAsync(eventId);

        // Act
        booking.Reject();
        @event.ReleaseSeats();

        // Assert
        Assert.Equal(seats, @event.AvailableSeats);

        // Act + Assert
        await BookingService.CreateBookingAsync(eventId);
    }
}