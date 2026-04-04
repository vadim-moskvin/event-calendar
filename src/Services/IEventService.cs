using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventService
{
    IEnumerable<Event> GetEvents();
    Event? GetEvent(Guid id);
    void AddEvent(Event @event);
    CreateOrUpdateResult<Event> ChangeEvent(Event @event);
    bool RemoveEvent(Guid id);
}