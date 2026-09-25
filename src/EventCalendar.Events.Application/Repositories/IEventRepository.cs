using EventCalendar.Events.Domain.Models;

namespace EventCalendar.Events.Application.Repositories;

public interface IEventRepository
{
    Task<PaginatedResult<Event>> GetEventsAsync(string? title, DateTime? from, DateTime? to, int page, int pageSize);

    Task<Event?> GetEventAsync(Guid id);

    Task<Event> CreateEventAsync(Event eventToCreate);

    Task<bool> RemoveEventAsync(Guid id);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}