using EventCalendar.Events.Application;
using EventCalendar.Events.Application.Repositories;
using EventCalendar.Events.Domain.Models;
using EventCalendar.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Events.Infrastructure.Repositories;

public class EventRepository(EventsDbContext appDbContext) : IEventRepository
{
    public async Task<PaginatedResult<Event>> GetEventsAsync(string? title, DateTime? from, DateTime? to, int page,
        int pageSize)
    {
        IQueryable<Event> query = appDbContext.Events;

        if (title != null)
            query = query.Where(e => e.Title.Contains(title));

        if (from.HasValue)
            query = query.Where(e => e.StartAt >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.EndAt <= to.Value);

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new PaginatedResult<Event>(items, page, totalPages, totalItems);
    }

    public async Task<Event?> GetEventAsync(Guid id)
    {
        return await appDbContext.Events.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Event> CreateEventAsync(Event eventToCreate)
    {
        await appDbContext.Events.AddAsync(eventToCreate);
        return eventToCreate;
    }

    public async Task<bool> RemoveEventAsync(Guid id)
    {
        var eventToRemove = await appDbContext.Events.FirstOrDefaultAsync(x => x.Id == id);
        if (eventToRemove == null) return false;
        appDbContext.Events.Remove(eventToRemove);
        return true;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }
}
