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

    public void AddEvent(Event @event)
    {
        if (!_events.TryAdd(@event.Id, @event))
            throw new EventAlreadyExistsException(@event.Id);
    }

    public CreateOrUpdateResult<Event> ChangeEvent(Event @event)
    {
        if (!_events.TryGetValue(@event.Id, out var original))
            return new CreateOrUpdateResult<Event>(@event, true);

        original.Title = @event.Title;
        original.Description = @event.Description;
        original.StartAt = @event.StartAt;
        original.EndAt = @event.EndAt;
        return new CreateOrUpdateResult<Event>(original, false);
    }

    public bool RemoveEvent(Guid id)
    {
        return _events.Remove(id);
    }
}