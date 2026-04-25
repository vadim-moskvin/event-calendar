using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventService
{
    PaginatedResult<Event> GetEvents(string? title, DateTime? from, DateTime? to, int page,
        int pageSize);

    Event? GetEvent(Guid id);
    bool AddEvent(Event @event);
    bool ChangeEvent(Event @event);
    bool RemoveEvent(Guid id);
}