using EventCalendar.Contracts;
using EventCalendar.Events.Application.Repositories;
using EventCalendar.Events.Application.Services;
using EventCalendar.Events.Domain.Models;
using EventCalendar.Events.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Events.Tests;

public sealed class BookingConfirmationHandlerTests : TestsBase
{
    [Fact]
    public async Task Confirm_booking()
    {
        // Arrange
        var @event = NewEvent(5);
        await EventService.AddEventAsync(@event);
        var handler = new BookingConfirmationHandler(
            ServiceProvider.GetRequiredService<IEventRepository>());

        // Act
        var result = await handler.HandleAsync(Confirmation(@event.Id, 2));

        // Assert
        Assert.Equal(BookingConfirmationResult.Reserved, result);
        using var scope = ServiceProvider.CreateScope();
        var saved = await scope.ServiceProvider
            .GetRequiredService<IEventRepository>()
            .GetEventAsync(@event.Id);
        Assert.Equal(3, saved?.AvailableSeats);
    }

    [Fact]
    public async Task Confirm_booking_for_missing_event()
    {
        // Arrange
        var handler = new BookingConfirmationHandler(
            ServiceProvider.GetRequiredService<IEventRepository>());

        // Act
        var result = await handler.HandleAsync(Confirmation(Guid.NewGuid(), 1));

        // Assert
        Assert.Equal(BookingConfirmationResult.EventNotFound, result);
    }

    [Fact]
    public async Task Confirm_booking_without_enough_seats()
    {
        // Arrange
        var @event = NewEvent(1);
        await EventService.AddEventAsync(@event);
        var handler = new BookingConfirmationHandler(
            ServiceProvider.GetRequiredService<IEventRepository>());

        // Act
        var result = await handler.HandleAsync(Confirmation(@event.Id, 2));

        // Assert
        Assert.Equal(BookingConfirmationResult.NoAvailableSeats, result);
        Assert.Equal(1, @event.AvailableSeats);
    }

    [Fact]
    public async Task Confirm_booking_with_invalid_seat_count()
    {
        // Arrange
        var @event = NewEvent(5);
        await EventService.AddEventAsync(@event);
        var handler = new BookingConfirmationHandler(
            ServiceProvider.GetRequiredService<IEventRepository>());

        // Act
        var result = await handler.HandleAsync(Confirmation(@event.Id, 0));

        // Assert
        Assert.Equal(BookingConfirmationResult.InvalidMessage, result);
        Assert.Equal(5, @event.AvailableSeats);
    }

    private static Event NewEvent(int seats) => new(
        Guid.NewGuid(), "Booking event", DateTime.UtcNow.AddDays(1),
        DateTime.UtcNow.AddDays(1).AddHours(1), seats);

    private static BookingConfirmed Confirmation(Guid eventId, int seats) => new(
        Guid.NewGuid(), eventId, Guid.NewGuid(), seats, DateTime.UtcNow);
}
