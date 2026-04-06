using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventService
{
    IEnumerable<Event> GetEvents();
    Event? GetEvent(Guid id);
    bool AddEvent(Event @event);
    bool ChangeEvent(Event @event);
    bool RemoveEvent(Guid id);
}