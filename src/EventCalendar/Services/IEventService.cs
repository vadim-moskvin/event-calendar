using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventService
{
    IEnumerable<Event> GetEvents(string? title = null,
        DateTime? from = null, DateTime? to = null);

    Event? GetEvent(Guid id);
    bool AddEvent(Event @event);
    bool ChangeEvent(Event @event);
    bool RemoveEvent(Guid id);
}