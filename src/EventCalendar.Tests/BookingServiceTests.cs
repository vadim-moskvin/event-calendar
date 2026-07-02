using EventCalendar.Exceptions;
using EventCalendar.Models;
using EventCalendar.Services;
using EventCalendar.Tests.TestHelpers;

namespace EventCalendar.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task Book_event()
    {
        // Arrange
        var eventService = TestServiceFactory.MakeEventService();
        var eventId = Guid.NewGuid();
        eventService.AddEvent(new Event(eventId, "Тестовое событие", DateTime.Today,
            DateTime.Today + TimeSpan.FromHours(1), 5, "Тестовое описание"));

        var bookingRepository = new InMemoryBookingStore();
        var bookingService = new BookingService(eventService, bookingRepository);

        // Act
        var booking = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotNull(booking);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
    }

    [Fact]
    public async Task Multiply_book_event()
    {
        // Arrange
        var eventService = TestServiceFactory.MakeEventService();
        var eventId = Guid.NewGuid();
        eventService.AddEvent(new Event(eventId, "Тестовое событие", DateTime.Today,
            DateTime.Today + TimeSpan.FromHours(1), 5, "Тестовое описание"));

        var bookingRepository = new InMemoryBookingStore();
        var bookingService = new BookingService(eventService, bookingRepository);

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
        var eventService = TestServiceFactory.MakeEventService();
        var eventId = Guid.NewGuid();
        eventService.AddEvent(new Event(eventId, "Тестовое событие", DateTime.Today,
            DateTime.Today + TimeSpan.FromHours(1), 5, "Тестовое описание"));

        var bookingRepository = new InMemoryBookingStore();
        var bookingService = new BookingService(eventService, bookingRepository);
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

        var bookingRepository = new InMemoryBookingStore();
        var bookingService = new BookingService(eventService, bookingRepository);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Book_deleted_event()
    {
        // Arrange
        var eventService = TestServiceFactory.MakeEventService();
        var eventId = Guid.NewGuid();
        eventService.AddEvent(new Event(eventId, "Тестовое событие", DateTime.Today,
            DateTime.Today + TimeSpan.FromHours(1), 5, "Тестовое описание"));
        eventService.RemoveEvent(eventId);

        var bookingRepository = new InMemoryBookingStore();
        var bookingService = new BookingService(eventService, bookingRepository);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Get_non_existing_booking()
    {
        // Arrange
        var eventService = TestServiceFactory.MakeEventService();
        var bookingRepository = new InMemoryBookingStore();
        var bookingService = new BookingService(eventService, bookingRepository);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await bookingService.GetBookingByIdAsync(Guid.NewGuid()));
    }
}