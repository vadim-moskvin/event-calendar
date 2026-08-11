using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventService
{
    PaginatedResult<Event> GetEvents(string? title, DateTime? from, DateTime? to, int page,
        int pageSize);

    Task<Event> GetEventAsync(Guid id);
    Task<bool> AddEventAsync(Event @event);
    Task ChangeEventAsync(Event @event);
    Task RemoveEventAsync(Guid id);
}