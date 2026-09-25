using EventCalendar.Events.Domain.Exceptions;
using EventCalendar.Events.Domain.Models;
using EventCalendar.Events.Tests.TestHelpers;

namespace EventCalendar.Events.Tests;

public class EventServiceTests : TestsBase
{
    [Fact]
    public async Task Add_new_event()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");

        // Act
        var result = await EventService.AddEventAsync(newEvent);

        // Assert
        var @event = await EventService.GetEventAsync(newEvent.Id);
        Assert.True(result);
        Assert.NotNull(@event);
        Assert.Equivalent(newEvent, @event);
    }

    [Fact]
    public async Task Add_new_event_Id_already_exists()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var newEvent = new Event(guid, "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        await EventService.AddEventAsync(newEvent);

        // Act
        var result = await EventService.AddEventAsync(newEvent);

        // Assert
        var events = await EventService.GetEventsAsync(null, null, null, page: 1, pageSize: 10);
        Assert.False(result);
        Assert.Single(events.Items);
        Assert.Equivalent(newEvent, events.Items.First());
    }

    [Fact]
    public async Task Get_all_events()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        await EventService.AddEventAsync(newEvent);
        await EventService.AddEventAsync(newEvent2);

        // Act
        var events = await EventService.GetEventsAsync(null, null, null, page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, events.Items.Count());
        Assert.NotStrictEqual(events.Items.ElementAt(0), events.Items.ElementAt(1));
    }

    [Fact]
    public async Task Get_event_by_id()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var newEvent = new Event(guid, "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        await EventService.AddEventAsync(newEvent);

        // Act
        var @event = await EventService.GetEventAsync(guid);

        // Assert
        Assert.Equivalent(newEvent, @event);
    }

    [Fact]
    public async Task Update_event()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var newEvent = new Event(guid, "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        var newEvent2 = new Event(guid, "Тестовое событие 2", new DateTime(2025, 10, 8),
            new DateTime(2026, 10, 9), 5, "Ещё одно описание");
        await EventService.AddEventAsync(newEvent);

        // Act
        await EventService.ChangeEventAsync(newEvent2);

        // Assert
        var @event = await EventService.GetEventAsync(newEvent.Id);
        Assert.NotNull(@event);
        Assert.Equal(newEvent2.Id, @event.Id);
        Assert.Equal(newEvent2.Title, @event.Title);
        Assert.Equal(newEvent2.Description, @event.Description);
        Assert.Equal(newEvent2.StartAt, @event.StartAt);
        Assert.Equal(newEvent2.EndAt, @event.EndAt);

        Assert.Equal(newEvent2.TotalSeats, @event.TotalSeats);
        Assert.Equal(newEvent2.AvailableSeats, @event.AvailableSeats);
    }

    [Fact]
    public async Task Updating_capacity_updates_available_seats()
    {
        var id = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(1);
        var original = new Event(id, "Концерт", start, start.AddHours(1), 5);
        await EventService.AddEventAsync(original);

        var updated = new Event(id, "Концерт", start, start.AddHours(1), 8);
        await EventService.ChangeEventAsync(updated);

        var saved = await EventService.GetEventAsync(id);
        Assert.Equal(8, saved.TotalSeats);
        Assert.Equal(8, saved.AvailableSeats);
    }

    [Fact]
    public async Task Delete_event()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        await EventService.AddEventAsync(newEvent);

        // Act
        await EventService.RemoveEventAsync(newEvent.Id);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await EventService.GetEventAsync(newEvent.Id));
    }

    [Fact]
    public async Task Filter_by_name()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        await EventService.AddEventAsync(newEvent);
        await EventService.AddEventAsync(newEvent2);

        // Act
        var events = await EventService.GetEventsAsync("2", null, null, page: 1, pageSize: 10);

        // Assert
        Assert.Single(events.Items);
        Assert.Equivalent(newEvent2, events.Items.First());
    }

    [Fact]
    public async Task Filter_by_from_date()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2025, 10, 9), 5, "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), 5, "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), 5, "Описание 3");
        await EventService.AddEventAsync(newEvent);
        await EventService.AddEventAsync(newEvent2);
        await EventService.AddEventAsync(newEvent3);

        // Act
        var events = await EventService.GetEventsAsync(null, new DateTime(2023, 1, 1), null, page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, events.Items.Count());
        Assert.Contains(events.Items, e => e.Id == newEvent2.Id);
        Assert.Contains(events.Items, e => e.Id == newEvent3.Id);
    }

    [Fact]
    public async Task Filter_by_to_date()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), 5, "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), 5, "Описание 3");
        await EventService.AddEventAsync(newEvent);
        await EventService.AddEventAsync(newEvent2);
        await EventService.AddEventAsync(newEvent3);

        // Act
        var events = await EventService.GetEventsAsync(null, null, new DateTime(2025, 12, 31), page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, events.Items.Count());
        Assert.Contains(events.Items, e => e.Id == newEvent.Id);
        Assert.Contains(events.Items, e => e.Id == newEvent2.Id);
    }

    [Fact]
    public async Task Paginate()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), 5, "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), 5, "Описание 3");
        await EventService.AddEventAsync(newEvent);
        await EventService.AddEventAsync(newEvent2);
        await EventService.AddEventAsync(newEvent3);

        // Act
        var result = await EventService.GetEventsAsync(null, null, null, page: 1, pageSize: 2);
        var result2 = await EventService.GetEventsAsync(null, null, null, page: 2, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Contains(result.Items, e => e.Id == newEvent.Id);
        Assert.Contains(result.Items, e => e.Id == newEvent2.Id);
        Assert.Single(result2.Items);
        Assert.Contains(result2.Items, e => e.Id == newEvent3.Id);
    }

    [Fact]
    public async Task Combine_filter()
    {
        // Arrange
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), 5, "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), 5, "Описание 3");
        await EventService.AddEventAsync(newEvent);
        await EventService.AddEventAsync(newEvent2);
        await EventService.AddEventAsync(newEvent3);

        // Act
        var result = await EventService.GetEventsAsync("3", new DateTime(2023, 1, 1),
            new DateTime(2027, 12, 31), page: 1, pageSize: 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equivalent(result.Items.First(), newEvent3);
    }

    [Fact]
    public async Task Get_non_existing_event()
    {
        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => EventService.GetEventAsync(@Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_non_existing_event()
    {
        // Arrange
        var @event = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), 5, "Описание");

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => EventService.ChangeEventAsync(@event));
    }

    [Fact]
    public void Create_invalid_event()
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(() => new Event(Guid.NewGuid(), string.Empty, new DateTime(2024, 10, 8),
            new DateTime(2022, 10, 9), 5, "Описание"));
    }
}
