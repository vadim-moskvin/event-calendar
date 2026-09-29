using System.Text.Json;
using EventCalendar.Events.Domain.Models;

namespace EventCalendar.Events.Tests;

public sealed class EventCacheSerializationTests
{
    [Fact]
    public void Event_with_reserved_seats_round_trip()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var @event = new Event(Guid.NewGuid(), "Concert", start, start.AddHours(1), 5);
        @event.TryReserveSeats();

        // Act
        var json = JsonSerializer.Serialize(@event);
        var restored = JsonSerializer.Deserialize<Event>(json);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(@event.Id, restored.Id);
        Assert.Equal(5, restored.TotalSeats);
        Assert.Equal(4, restored.AvailableSeats);
    }
}
