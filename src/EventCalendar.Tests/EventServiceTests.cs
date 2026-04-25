using EventCalendar.Models;
using EventCalendar.Services;

namespace EventCalendar.Tests;

public class EventServiceTests
{
    [Fact]
    public void Add_new_event()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");

        // Act
        var result = service.AddEvent(newEvent);

        // Assert
        var @event = service.GetEvent(newEvent.Id);
        Assert.True(result);
        Assert.NotNull(@event);
        Assert.Equivalent(newEvent, @event);
    }

    [Fact]
    public void Add_new_event_Id_already_exists()
    {
        // Arrange
        var service = new EventService();
        var guid = Guid.NewGuid();
        var newEvent = new Event(guid, "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(guid, "Тестовое событие 2", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        service.AddEvent(newEvent);

        // Act
        var result = service.AddEvent(newEvent);

        // Assert
        var events = service.GetEvents(null, null, null, page: 1, pageSize: 10);
        Assert.False(result);
        Assert.Single(events.Items);
        Assert.Equivalent(newEvent, events.Items.First());
    }

    [Fact]
    public void Get_all_events()
    {
        // Arrange
        var service = new EventService();
        var guid = Guid.NewGuid();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        service.AddEvent(newEvent);
        service.AddEvent(newEvent2);

        // Act
        var events = service.GetEvents(null, null, null, page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, events.Items.Count());
        Assert.NotStrictEqual(events.Items.ElementAt(0), events.Items.ElementAt(1));
    }

    [Fact]
    public void Get_event_by_id()
    {
        // Arrange
        var service = new EventService();
        var guid = Guid.NewGuid();
        var newEvent = new Event(guid, "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        service.AddEvent(newEvent);

        // Act
        var @event = service.GetEvent(guid);

        // Assert
        Assert.Equivalent(newEvent, @event);
    }

    [Fact]
    public void Update_event()
    {
        // Arrange
        var service = new EventService();
        var guid = Guid.NewGuid();
        var newEvent = new Event(guid, "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(guid, "Тестовое событие 2", new DateTime(2025, 10, 8),
            new DateTime(2026, 10, 9), "Ещё одно описание");
        service.AddEvent(newEvent);

        // Act
        var result = service.ChangeEvent(newEvent2);

        // Assert
        var @event = service.GetEvent(newEvent.Id);
        Assert.True(result);
        Assert.Equivalent(newEvent2, @event);
    }

    [Fact]
    public void Delete_event()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        service.AddEvent(newEvent);

        // Act
        var result = service.RemoveEvent(newEvent.Id);

        // Assert
        var @event = service.GetEvent(newEvent.Id);
        Assert.True(result);
        Assert.Null(@event);
    }

    [Fact]
    public void Filter_by_name()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2024, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        service.AddEvent(newEvent);
        service.AddEvent(newEvent2);

        // Act
        var events = service.GetEvents("2", null, null, page: 1, pageSize: 10);

        // Assert
        Assert.Single(events.Items);
        Assert.Equivalent(newEvent2, events.Items.First());
    }

    [Fact]
    public void Filter_by_from_date()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2025, 10, 9), "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), "Описание 3");
        service.AddEvent(newEvent);
        service.AddEvent(newEvent2);
        service.AddEvent(newEvent3);

        // Act
        var events = service.GetEvents(null, new DateTime(2023, 1, 1), null, page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, events.Items.Count());
        Assert.Contains(events.Items, e => e.Id == newEvent2.Id);
        Assert.Contains(events.Items, e => e.Id == newEvent3.Id);
    }

    [Fact]
    public void Filter_by_to_date()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), "Описание 3");
        service.AddEvent(newEvent);
        service.AddEvent(newEvent2);
        service.AddEvent(newEvent3);

        // Act
        var events = service.GetEvents(null, null, new DateTime(2025, 12, 31), page: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, events.Items.Count());
        Assert.Contains(events.Items, e => e.Id == newEvent.Id);
        Assert.Contains(events.Items, e => e.Id == newEvent2.Id);
    }

    [Fact]
    public void Paginate()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), "Описание 3");
        service.AddEvent(newEvent);
        service.AddEvent(newEvent2);
        service.AddEvent(newEvent3);

        // Act
        var result = service.GetEvents(null, null, null, page: 1, pageSize: 2);
        var result2 = service.GetEvents(null, null, null, page: 2, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Contains(result.Items, e => e.Id == newEvent.Id);
        Assert.Contains(result.Items, e => e.Id == newEvent2.Id);
        Assert.Single(result2.Items);
        Assert.Contains(result2.Items, e => e.Id == newEvent3.Id);
    }

    [Fact]
    public void Combine_filter()
    {
        // Arrange
        var service = new EventService();
        var newEvent = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), "Описание");
        var newEvent2 = new Event(Guid.NewGuid(), "Тестовое событие 2", new DateTime(2023, 10, 8),
            new DateTime(2025, 10, 9), "Описание 2");
        var newEvent3 = new Event(Guid.NewGuid(), "Тестовое событие 3", new DateTime(2024, 10, 8),
            new DateTime(2026, 10, 9), "Описание 3");
        service.AddEvent(newEvent);
        service.AddEvent(newEvent2);
        service.AddEvent(newEvent3);

        // Act
        var result = service.GetEvents("3", new DateTime(2023, 1, 1),
            new DateTime(2027, 12, 31), page: 1, pageSize: 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equivalent(result.Items.First(), newEvent3);
    }

    [Fact]
    public void Get_non_existing_event()
    {
        // Arrange
        var service = new EventService();

        // Act
        var result = service.GetEvent(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Update_non_existing_event()
    {
        // Arrange
        var service = new EventService();
        var @event = new Event(Guid.NewGuid(), "Тестовое событие", new DateTime(2022, 10, 8),
            new DateTime(2024, 10, 9), "Описание");

        // Act
        var result = service.ChangeEvent(@event);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Create_invalid_event()
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(() => new Event(Guid.NewGuid(), string.Empty, new DateTime(2024, 10, 8),
            new DateTime(2022, 10, 9), "Описание"));
    }
}