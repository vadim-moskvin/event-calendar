using EventCalendar.Models;
using EventCalendar.Repositories;
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
        var eventId = Guid.NewGuid();
        var @event = TestServiceFactory.MakeEvent(eventId);
        
        await context.Events.AddAsync(@event);
        await context.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var repository = new BookingRepository(actContext);
        var booking = await repository.CreateBookingAsync(Booking.MakeNew(eventId));
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
        var eventId = Guid.NewGuid();
        const string title = "Концерт";
        
        context.Events.Add(TestServiceFactory.MakeEvent(eventId, title));
        var booking = Booking.MakeNew(eventId);
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
    public async Task Create_booking_for_invalid_event()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var repository = new BookingRepository(context);
        await repository.CreateBookingAsync(Booking.MakeNew(Guid.NewGuid()));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
    }
}