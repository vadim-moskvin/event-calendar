using EventCalendar.DataAccess;
using EventCalendar.Exceptions;
using EventCalendar.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Services;

public class EventService(AppDbContext appDbContext) : IEventService
{
    private const string DateOutOfRangeException = "Параметр {0} не может быть больше параметра {1}.";
    private const string PageOutOfRangeException = "Номер страницы должен быть больше ноля.";
    private const string PageSizeOutOfRangeException = "Размер страницы должен быть больше ноля.";
    private const string EventNotFoundException = "Событие не найдено";

    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;

    public PaginatedResult<Event> GetEvents(string? title, DateTime? from, DateTime? to, int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new BadRequestException(string.Format(DateOutOfRangeException, nameof(from), nameof(to)));

        if (page < 1)
            throw new BadRequestException(PageOutOfRangeException);

        if (pageSize < 1)
            throw new BadRequestException(PageSizeOutOfRangeException);

        IQueryable<Event> query = appDbContext.Events;

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(e =>
                EF.Functions.ILike(e.Title, $"%{title}%"));
        }

        if (from.HasValue)
            query = query.Where(e => e.StartAt >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.EndAt <= to.Value);

        var filtered = query.ToArray();

        var items = query
            .OrderBy(c => c.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var totalPages = (int)Math.Ceiling((double)filtered.Length / pageSize);

        return new PaginatedResult<Event>(items, page, totalPages, filtered.Length);
    }

    public async Task<Event> GetEventAsync(Guid id)
    {
        return await appDbContext.Events.FirstOrDefaultAsync(x => x.Id == id) ??
               throw new NotFoundException(EventNotFoundException);
    }

    public async Task<bool> AddEventAsync(Event @event)
    {
        await appDbContext.Events.AddAsync(@event);
        return await appDbContext.SaveChangesAsync() > 0;
    }

    public async Task ChangeEventAsync(Event @event)
    {
        var entity = await appDbContext.Events.FirstOrDefaultAsync(x => x.Id == @event.Id);
        if (entity == null)
            throw new NotFoundException(EventNotFoundException);
        
        appDbContext.Events.Update(@event);
        await appDbContext.SaveChangesAsync();
    }

    public async Task RemoveEventAsync(Guid id)
    {
        var entity = await appDbContext.Events.FindAsync(id);
        
        if (entity == null)
            throw new NotFoundException(EventNotFoundException);
        
        appDbContext.Events.Remove(entity);
        await appDbContext.SaveChangesAsync();
    }
}