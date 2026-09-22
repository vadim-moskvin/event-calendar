using EventCalendar.Domain.Models;
using EventCalendar.Infrastructure.Repositories;
using EventCalendar.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.IntegrationTests;

public class BookingRepositoryTests : TestsBase
{
    [Fact]
    public async Task Create_booking()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();

        var user = TestServiceFactory.MakeUser();
        await context.Users.AddAsync(user);

        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(eventId);
        await context.Events.AddAsync(@event);

        await context.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var repository = new BookingRepository(actContext);
        var booking = await repository.CreateBookingAsync(Booking.MakeNew(user.Id, eventId));
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Get_booking()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();

        var user = TestServiceFactory.MakeUser();
        context.Users.Add(user);

        var eventId = Guid.NewGuid();
        const string title = "Концерт";
        context.Events.Add(TestServiceFactory.MakeEvent(eventId, title));

        var booking = Booking.MakeNew(user.Id, eventId);
        await context.Bookings.AddAsync(booking);

        await context.SaveChangesAsync();

        // Act
        var repository = new BookingRepository(CreateContext());
        var result = await repository.GetBookingAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Event.Title);
    }

    [Fact]
    public async Task Find_pending_bookings()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();

        var user = TestServiceFactory.MakeUser();
        await context.Users.AddAsync(user);

        var eventId = Guid.NewGuid();
        const string title = "Концерт";
        context.Events.Add(TestServiceFactory.MakeEvent(eventId, title));

        var booking1 = TestServiceFactory.MakeBooking(user.Id, eventId);
        await context.Bookings.AddAsync(booking1);
        var booking2 = TestServiceFactory.MakeBooking(user.Id, eventId);
        booking2.Confirm();
        await context.Bookings.AddAsync(booking2);

        await context.SaveChangesAsync();

        // Act
        var repository = new BookingRepository(CreateContext());
        var result = await repository.GetPendingBookingsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(booking1.Id, result.Single().Id);
        Assert.NotNull(result.Single().Event);
    }

    [Fact]
    public async Task Create_booking_for_invalid_event()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var repository = new BookingRepository(context);
        await repository.CreateBookingAsync(TestServiceFactory.MakeBooking());

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
    }
}