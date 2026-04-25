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
        if (!_events.ContainsKey(@event.Id))
            return false;
        
        _events[@event.Id] = @event;
        return true;
    }

    public bool RemoveEvent(Guid id)
    {
        return _events.Remove(id);
    }
}