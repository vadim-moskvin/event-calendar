using EventCalendar.Application;
using EventCalendar.Domain.Models;
using EventCalendar.Infrastructure.Repositories;
using EventCalendar.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.IntegrationTests;

public class EventRepositoryTests : TestsBase
{
    [Fact]
    public async Task Create_event()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var id = Guid.NewGuid();
        const string title = "Концерт";
        var @event = TestServiceFactory.MakeEvent(id, title);

        // Act
        await repository.CreateEventAsync(@event);
        await repository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events
            .FirstOrDefaultAsync(b => b.Id == id);

        Assert.NotNull(saved);
        Assert.Equal(title, saved.Title);
    }

    [Fact]
    public async Task Get_event()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var id = Guid.NewGuid();
        const string title = "Концерт";
        context.Events.Add(TestServiceFactory.MakeEvent(id, title));
        await context.SaveChangesAsync();

        // Act
        var repository = new EventRepository(CreateContext());
        var result = await repository.GetEventAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
    }

    [Fact]
    public async Task Find_events_by_title()
    {
        await ResetDatabaseAsync();

        // Arrange
        var id = Guid.NewGuid();
        await using (var arrangeContext = CreateContext())
        {
            arrangeContext.Events.AddRange(
                TestServiceFactory.MakeEvent(title: "Джазовый концерт"),
                TestServiceFactory.MakeEvent(id, "Рок-концерт"));
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        PaginatedResult<Event> result;
        await using (var actContext = CreateContext())
        {
            var repository = new EventRepository(actContext);
            result = await repository.GetEventsAsync("Рок", null, null, page: 1, pageSize: 10);
        }

        // Assert
        var @event = Assert.Single(result.Items);
        Assert.Equal(id, @event.Id);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task Find_events_starting_from_date()
    {
        await ResetDatabaseAsync();

        // Arrange
        var id = Guid.NewGuid();
        var from = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        await using (var arrangeContext = CreateContext())
        {
            arrangeContext.Events.AddRange(
                TestServiceFactory.MakeEvent(startAt: from.AddMinutes(-1), endAt: from.AddHours(1)),
                TestServiceFactory.MakeEvent(id, startAt: from, endAt: from.AddHours(1)));
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        PaginatedResult<Event> result;
        await using (var actContext = CreateContext())
        {
            var repository = new EventRepository(actContext);
            result = await repository.GetEventsAsync(null, from, null, page: 1, pageSize: 10);
        }

        // Assert
        var @event = Assert.Single(result.Items);
        Assert.Equal(id, @event.Id);
    }

    [Fact]
    public async Task Find_events_ending_before_date()
    {
        await ResetDatabaseAsync();

        // Arrange
        var id = Guid.NewGuid();
        var to = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        await using (var arrangeContext = CreateContext())
        {
            arrangeContext.Events.AddRange(
                TestServiceFactory.MakeEvent(id, startAt: to.AddHours(-2), endAt: to),
                TestServiceFactory.MakeEvent(startAt: to.AddHours(-1), endAt: to.AddMinutes(1)));
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        PaginatedResult<Event> result;
        await using (var actContext = CreateContext())
        {
            var repository = new EventRepository(actContext);
            result = await repository.GetEventsAsync(null, null, to, page: 1, pageSize: 10);
        }

        // Assert
        var @event = Assert.Single(result.Items);
        Assert.Equal(id, @event.Id);
    }

    [Fact]
    public async Task Find_events_with_all_filters()
    {
        await ResetDatabaseAsync();

        // Arrange
        var id = Guid.NewGuid();
        var from = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 12, 12, 0, 0, DateTimeKind.Utc);
        await using (var arrangeContext = CreateContext())
        {
            arrangeContext.Events.AddRange(
                TestServiceFactory.MakeEvent(title: "Рок-концерт", startAt: from.AddDays(-1), endAt: to),
                TestServiceFactory.MakeEvent(title: "Джазовый концерт", startAt: from, endAt: to),
                TestServiceFactory.MakeEvent(title: "Рок-концерт", startAt: from, endAt: to.AddDays(1)),
                TestServiceFactory.MakeEvent(id, "Рок-концерт", from, to));
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        PaginatedResult<Event> result;
        await using (var actContext = CreateContext())
        {
            var repository = new EventRepository(actContext);
            result = await repository.GetEventsAsync("Рок", from, to, page: 1, pageSize: 10);
        }

        // Assert
        var @event = Assert.Single(result.Items);
        Assert.Equal(id, @event.Id);
    }

    [Fact]
    public async Task Return_requested_page()
    {
        await ResetDatabaseAsync();

        // Arrange
        var secondPageId = Guid.NewGuid();
        var start = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        await using (var arrangeContext = CreateContext())
        {
            arrangeContext.Events.AddRange(
                TestServiceFactory.MakeEvent(startAt: start, endAt: start.AddHours(1)),
                TestServiceFactory.MakeEvent(startAt: start.AddDays(1), endAt: start.AddDays(1).AddHours(1)),
                TestServiceFactory.MakeEvent(secondPageId, startAt: start.AddDays(2), endAt: start.AddDays(2).AddHours(1)));
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        PaginatedResult<Event> result;
        await using (var actContext = CreateContext())
        {
            var repository = new EventRepository(actContext);
            result = await repository.GetEventsAsync(null, null, null, page: 2, pageSize: 2);
        }

        // Assert
        var @event = Assert.Single(result.Items);
        Assert.Equal(secondPageId, @event.Id);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task Update_event()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var arrangeContext = CreateContext();
        var id = Guid.NewGuid();

        arrangeContext.Events.Add(TestServiceFactory.MakeEvent(id, "Спектакль"));
        await arrangeContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var eventRepository = new EventRepository(actContext);
        var @event = await eventRepository.GetEventAsync(id);
        var startAt = DateTime.UtcNow;
        var endAt = DateTime.UtcNow.AddHours(1);
        @event!.Update("Концерт", "Для взрослых", startAt, endAt);
        await eventRepository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var updated = await verifyContext.Events.FirstAsync(x => x.Id == id);
        Assert.Equal("Концерт", updated.Title);
        Assert.Equal("Для взрослых", updated.Description);
    }

    [Fact]
    public async Task Delete_event()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var id = Guid.NewGuid();

        context.Events.Add(TestServiceFactory.MakeEvent(id));
        await context.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var eventRepository = new EventRepository(actContext);
        await eventRepository.RemoveEventAsync(id);
        await eventRepository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var deleted = await verifyContext.Events.FirstOrDefaultAsync(x => x.Id == id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Create_duplicate()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var id = Guid.NewGuid();
        const string title = "Концерт";
        var @event = TestServiceFactory.MakeEvent(id, title);

        await context.Events.AddAsync(@event);
        await context.SaveChangesAsync();

        var duplicate = TestServiceFactory.MakeEvent(id, title);

        // Act + Assert
        await using var verifyContext = CreateContext();
        var repository = new EventRepository(verifyContext);
        await repository.CreateEventAsync(duplicate);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
    }

    [Fact]
    public async Task Get_event_with_bookings()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var eventId = Guid.NewGuid();
        const string title = "Концерт";
        context.Events.Add(TestServiceFactory.MakeEvent(eventId, title));
        context.Bookings.Add(Booking.MakeNew(eventId));
        context.Bookings.Add(Booking.MakeNew(eventId));
        context.Bookings.Add(Booking.MakeNew(eventId));
        await context.SaveChangesAsync();

        // Act
        var repository = new EventRepository(CreateContext());
        var result = await repository.GetEventAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(3, result.Bookings.Count);
    }

    [Fact]
    public async Task Delete_event_with_booking()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var eventId = Guid.NewGuid();
        await context.Events.AddAsync(TestServiceFactory.MakeEvent(eventId));
        await context.Bookings.AddAsync(Booking.MakeNew(eventId));
        await context.SaveChangesAsync();

        // Act
        await using var actContext = CreateContext();
        var eventRepository = new EventRepository(actContext);
        await eventRepository.RemoveEventAsync(eventId);
        await eventRepository.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var bookings = await verifyContext.Bookings.Where(x => x.EventId == eventId).ToListAsync();
        Assert.Empty(bookings);
    }
}
