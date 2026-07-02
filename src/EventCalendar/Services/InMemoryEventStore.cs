using EventCalendar.Models;

namespace EventCalendar.Services;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, Event> _events = [];

    public IList<Event> Get()
    {
        return _events.Values.ToArray();
    }

    public Event? Get(Guid id)
    {
        return _events.GetValueOrDefault(id);
    }

    public bool Add(Event @event)
    {
        return _events.TryAdd(@event.Id, @event);
    }

    public bool Update(Event @event)
    {
        if (!_events.ContainsKey(@event.Id))
            return false;
        
        _events[@event.Id] = @event;
        
        return true;
    }

    public bool Remove(Guid id)
    {
        return _events.Remove(id);
    }
}