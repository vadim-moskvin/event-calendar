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
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = await BookingService.CreateBookingAsync(userId, eventId);
        var saved = await BookingService.GetBookingByIdAsync(booking.Id, userId, false);

        Assert.Equal(userId, saved.UserId);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task Reject_eleventh_active_booking()
    {
        var userId = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
            await BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        await Assert.ThrowsAsync<MaxBookingPerUserException>(() =>
            BookingService.CreateBookingAsync(userId, Guid.NewGuid()));
    }

    [Fact]
    public async Task Limit_is_per_user()
    {
        var firstUser = Guid.NewGuid();
        var secondUser = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
            await BookingService.CreateBookingAsync(firstUser, Guid.NewGuid());

        var booking = await BookingService.CreateBookingAsync(secondUser, Guid.NewGuid());

        Assert.Equal(secondUser, booking.UserId);
    }

    [Fact]
    public async Task Cancelled_booking_frees_a_slot()
    {
        var userId = Guid.NewGuid();
        var first = await BookingService.CreateBookingAsync(userId, Guid.NewGuid());
        for (var i = 1; i < 10; i++)
            await BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        await BookingService.CancelBookingAsync(first.Id, userId, false);
        var replacement = await BookingService.CreateBookingAsync(userId, Guid.NewGuid());

        Assert.Equal(BookingStatus.Pending, replacement.Status);
        Assert.Equal(BookingStatus.Cancelled,
            (await BookingService.GetBookingByIdAsync(first.Id, userId, false)).Status);
    }

    [Fact]
    public async Task Other_user_cannot_read_or_cancel_booking()
    {
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var booking = await BookingService.CreateBookingAsync(owner, Guid.NewGuid());

        await Assert.ThrowsAsync<NotAllowedException>(() =>
            BookingService.GetBookingByIdAsync(booking.Id, other, false));
        await Assert.ThrowsAsync<NotAllowedException>(() =>
            BookingService.CancelBookingAsync(booking.Id, other, false));
        Assert.Equal(BookingStatus.Pending,
            (await BookingService.GetBookingByIdAsync(booking.Id, owner, false)).Status);
    }

    [Fact]
    public async Task Admin_can_cancel_another_users_booking()
    {
        var booking = await BookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());

        await BookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), true);

        Assert.Equal(BookingStatus.Cancelled,
            (await BookingService.GetBookingByIdAsync(booking.Id, Guid.NewGuid(), true)).Status);
    }

    [Fact]
    public async Task Missing_booking_throws_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            BookingService.GetBookingByIdAsync(Guid.NewGuid(), Guid.NewGuid(), false));
    }

    [Fact]
    public async Task Concurrent_requests_for_same_user_observe_limit()
    {
        var userId = Guid.NewGuid();
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

        Assert.Equal(10, results.Count(x => x));
        Assert.Equal(10, results.Count(x => !x));
    }
}
