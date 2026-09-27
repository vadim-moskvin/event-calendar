using EventCalendar.Events.Application;
using EventCalendar.Events.Application.Repositories;
using EventCalendar.Events.Application.Services;
using EventCalendar.Events.Domain.Models;
using EventCalendar.Events.Tests.TestHelpers;
using Moq;

namespace EventCalendar.Events.Tests;

public sealed class EventServiceCacheTests
{
    private readonly Mock<IEventRepository> _repository = new(MockBehavior.Strict);
    private readonly StubCacheService _cache = new();
    private readonly EventService _service;

    public EventServiceCacheTests()
    {
        _service = new EventService(_repository.Object, _cache, new CacheSettings
        {
            GetEventTtlInMinutes = 1,
            GetTop10EventsTtlInMinutes = 10
        });
    }

    [Fact]
    public async Task Get_event_with_cached_value()
    {
        // Arrange
        var @event = NewEvent();
        var key = Constants.EventCacheKey(@event.Id);
        await _cache.Set(key, @event, TimeSpan.FromMinutes(1));

        // Act
        var result = await _service.GetEventAsync(@event.Id);

        // Assert
        Assert.Same(@event, result);
        Assert.Single(_cache.Sets);
        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_event_without_cached_value()
    {
        // Arrange
        var @event = NewEvent();
        _repository.Setup(repository => repository.GetEventAsync(@event.Id)).ReturnsAsync(@event);

        // Act
        var first = await _service.GetEventAsync(@event.Id);
        var second = await _service.GetEventAsync(@event.Id);

        // Assert
        Assert.Same(@event, first);
        Assert.Same(@event, second);
        _repository.Verify(repository => repository.GetEventAsync(@event.Id), Times.Once);
        _repository.VerifyNoOtherCalls();
        var set = Assert.Single(_cache.Sets);
        Assert.Equal(Constants.EventCacheKey(@event.Id), set.Key);
        Assert.Same(@event, set.Value);
        Assert.Equal(TimeSpan.FromMinutes(1), set.Expiration);
    }

    [Fact]
    public async Task Get_top10_with_cached_value()
    {
        // Arrange
        var events = new List<Event> { NewEvent() };
        await _cache.Set(Constants.Top10EventsCacheKey, events, TimeSpan.FromMinutes(10));

        // Act
        var result = await _service.GetTop10EventsAsync();

        // Assert
        Assert.Same(events, result);
        Assert.Single(_cache.Sets);
        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_top10_without_cached_value()
    {
        // Arrange
        var events = new List<Event> { NewEvent() };
        _repository.Setup(repository => repository.GetTop10EventsAsync()).ReturnsAsync(events);

        // Act
        var first = await _service.GetTop10EventsAsync();
        var second = await _service.GetTop10EventsAsync();

        // Assert
        Assert.Same(events, first);
        Assert.Same(events, second);
        _repository.Verify(repository => repository.GetTop10EventsAsync(), Times.Once);
        _repository.VerifyNoOtherCalls();
        var set = Assert.Single(_cache.Sets);
        Assert.Equal(Constants.Top10EventsCacheKey, set.Key);
        Assert.Same(events, set.Value);
        Assert.Equal(TimeSpan.FromMinutes(10), set.Expiration);
    }

    [Fact]
    public async Task Update_event_with_cached_value()
    {
        // Arrange
        var @event = NewEvent();
        var key = Constants.EventCacheKey(@event.Id);
        await _cache.Set(key, @event, TimeSpan.FromMinutes(1));
        var updated = new Event(@event.Id, "Updated", @event.StartAt, @event.EndAt, 8);
        _repository.Setup(repository => repository.GetEventAsync(@event.Id)).ReturnsAsync(@event);
        _repository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.ChangeEventAsync(updated);

        // Assert
        Assert.Equal("Updated", @event.Title);
        _repository.Verify(repository => repository.GetEventAsync(@event.Id), Times.Once);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repository.VerifyNoOtherCalls();
        Assert.Contains(key, _cache.RemovedKeys);
        Assert.Null(await _cache.Get<Event>(key));
    }

    [Fact]
    public async Task Delete_event_with_cached_value()
    {
        // Arrange
        var @event = NewEvent();
        var key = Constants.EventCacheKey(@event.Id);
        await _cache.Set(key, @event, TimeSpan.FromMinutes(1));
        _repository.Setup(repository => repository.RemoveEventAsync(@event.Id)).ReturnsAsync(true);
        _repository.Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.RemoveEventAsync(@event.Id);

        // Assert
        _repository.Verify(repository => repository.RemoveEventAsync(@event.Id), Times.Once);
        _repository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repository.VerifyNoOtherCalls();
        Assert.Contains(key, _cache.RemovedKeys);
        Assert.Null(await _cache.Get<Event>(key));
    }

    private static Event NewEvent() => new(
        Guid.NewGuid(), "Event", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), 5);
}
