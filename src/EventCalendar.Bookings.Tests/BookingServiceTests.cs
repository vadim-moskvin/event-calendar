using EventCalendar.Bookings.Application.Services;
using EventCalendar.Bookings.Domain.Exceptions;
using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Bookings.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.Tests;

public class BookingServiceTests : TestsBase
{
    [Fact]
    public async Task Create_booking_without_event_lookup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        // Act
        var booking = await BookingService.CreateBookingAsync(userId, eventId);
        var saved = await BookingService.GetBookingByIdAsync(booking.Id, userId, false);

        // Assert
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task Create_booking_at_user_limit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
            await BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        // Act
        var action = () => BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        // Assert
        await Assert.ThrowsAsync<MaxBookingPerUserException>(action);
    }

    [Fact]
    public async Task Create_booking_for_another_user_at_limit()
    {
        // Arrange
        var firstUser = Guid.NewGuid();
        var secondUser = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
            await BookingService.CreateBookingAsync(firstUser, Guid.NewGuid());

        // Act
        var booking = await BookingService.CreateBookingAsync(secondUser, Guid.NewGuid());

        // Assert
        Assert.Equal(secondUser, booking.UserId);
    }

    [Fact]
    public async Task Create_booking_after_cancellation_at_limit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var first = await BookingService.CreateBookingAsync(userId, Guid.NewGuid());
        for (var i = 1; i < 10; i++)
            await BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        // Act
        await BookingService.CancelBookingAsync(first.Id, userId, false);
        var replacement = await BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        // Assert
        Assert.Equal(BookingStatus.Pending, replacement.Status);
        Assert.Equal(BookingStatus.Cancelled,
            (await BookingService.GetBookingByIdAsync(first.Id, userId, false)).Status);
    }

    [Fact]
    public async Task Read_or_cancel_another_users_booking()
    {
        // Arrange
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var booking = await BookingService.CreateBookingAsync(owner, Guid.NewGuid());

        // Act
        var read = () => BookingService.GetBookingByIdAsync(booking.Id, other, false);
        var cancel = () => BookingService.CancelBookingAsync(booking.Id, other, false);

        // Assert
        await Assert.ThrowsAsync<NotAllowedException>(read);
        await Assert.ThrowsAsync<NotAllowedException>(cancel);
        Assert.Equal(BookingStatus.Pending,
            (await BookingService.GetBookingByIdAsync(booking.Id, owner, false)).Status);
    }

    [Fact]
    public async Task Cancel_another_users_booking_as_admin()
    {
        // Arrange
        var booking = await BookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await BookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), true);

        // Assert
        Assert.Equal(BookingStatus.Cancelled,
            (await BookingService.GetBookingByIdAsync(booking.Id, Guid.NewGuid(), true)).Status);
    }

    [Fact]
    public async Task Get_missing_booking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var action = () => BookingService.GetBookingByIdAsync(bookingId, userId, false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task Create_bookings_concurrently_for_same_user()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var tasks = Enumerable.Range(0, 20).Select(async _ =>
        {
            using var scope = ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
            try
            {
                await service.CreateBookingAsync(userId, Guid.NewGuid());
                return true;
            }
            catch (MaxBookingPerUserException)
            {
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Count(x => x));
        Assert.Equal(10, results.Count(x => !x));
    }
}
