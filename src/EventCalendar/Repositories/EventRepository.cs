using EventCalendar.Application;
using EventCalendar.Application.Repositories;
using EventCalendar.DataAccess;
using EventCalendar.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Repositories;

public class EventRepository(AppDbContext appDbContext) : IEventRepository
{
    public async Task<PaginatedResult<Event>> GetEventsAsync(string? title, DateTime? from, DateTime? to, int page,
        int pageSize)
    {
        IQueryable<Event> query = appDbContext.Events.Include(x => x.Bookings);

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
        return await appDbContext.Events
            .Include(x => x.Bookings)
            .FirstOrDefaultAsync(x => x.Id == id);
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