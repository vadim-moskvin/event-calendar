using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Bookings.IntegrationTests;

public class BookingRepositoryTests : TestsBase
{
    [Fact]
    public async Task Create_booking_without_event_or_user_foreign_keys()
    {
        await ResetDatabaseAsync();
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var repository = new BookingRepository(context);
            await repository.CreateBookingAsync(Booking.MakeNew(userId, eventId));
            await repository.SaveChangesAsync();
        }

        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Bookings.SingleAsync();
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal(BookingStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task Get_booking()
    {
        await ResetDatabaseAsync();
        var booking = Booking.MakeNew(Guid.NewGuid(), Guid.NewGuid());
        await using (var context = CreateContext())
        {
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        await using var actContext = CreateContext();
        var result = await new BookingRepository(actContext).GetBookingAsync(booking.Id);

        Assert.NotNull(result);
        Assert.Equal(booking.EventId, result.EventId);
        Assert.Equal(booking.UserId, result.UserId);
    }

    [Fact]
    public async Task Find_pending_bookings()
    {
        await ResetDatabaseAsync();
        var pending = Booking.MakeNew(Guid.NewGuid(), Guid.NewGuid());
        var confirmed = Booking.MakeNew(Guid.NewGuid(), Guid.NewGuid());
        confirmed.Confirm();
        await using (var context = CreateContext())
        {
            context.Bookings.AddRange(pending, confirmed);
            await context.SaveChangesAsync();
        }

        await using var actContext = CreateContext();
        var result = await new BookingRepository(actContext).GetPendingBookingsAsync();

        Assert.Equal(pending.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task Count_only_pending_and_confirmed_for_user()
    {
        await ResetDatabaseAsync();
        var userId = Guid.NewGuid();
        var pending = Booking.MakeNew(userId, Guid.NewGuid());
        var confirmed = Booking.MakeNew(userId, Guid.NewGuid());
        confirmed.Confirm();
        var cancelled = Booking.MakeNew(userId, Guid.NewGuid());
        cancelled.Cancel();
        var rejected = Booking.MakeNew(userId, Guid.NewGuid());
        rejected.Reject();
        var otherUser = Booking.MakeNew(Guid.NewGuid(), Guid.NewGuid());
        await using (var context = CreateContext())
        {
            context.Bookings.AddRange(pending, confirmed, cancelled, rejected, otherUser);
            await context.SaveChangesAsync();
        }

        await using var actContext = CreateContext();
        var count = await new BookingRepository(actContext).GetActiveBookingCountByUserIdAsync(userId);

        Assert.Equal(2, count);
    }
}
