using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventService
{
    Task<PaginatedResult<Event>> GetEventsAsync(string? title, DateTime? from, DateTime? to, int page,
        int pageSize);
    Task<Event> GetEventAsync(Guid id);
    Task<bool> AddEventAsync(Event @event);
    Task ChangeEventAsync(Event @event);
    Task RemoveEventAsync(Guid id);
}