using EventCalendar.Models;

namespace EventCalendar.Services;

public class EventService : IEventService
{
    private readonly Dictionary<Guid, Event> _events = [];

    public IEnumerable<Event> GetEvents()
    {
        return _events.Values.ToArray();
    }

    public Event? GetEvent(Guid id)
    {
        return _events.GetValueOrDefault(id);
    }

    public bool AddEvent(Event @event)
    {
        return _events.TryAdd(@event.Id, @event);
    }

    public bool ChangeEvent(Event @event)
    {
        if (!_events.TryGetValue(@event.Id, out var original))
            return false;

        original.Update(@event.Title, @event.Description, @event.StartAt, @event.EndAt);
        return true;
    }

    public bool RemoveEvent(Guid id)
    {
        return _events.Remove(id);
    }
}