using EventCalendar.Events.Application.Repositories;
using EventCalendar.Events.Domain.Exceptions;
using EventCalendar.Events.Domain.Models;

namespace EventCalendar.Events.Application.Services;

public class EventService(IEventRepository eventRepository, ICacheService cacheService) : IEventService
{
    private const string DateOutOfRangeException = "Параметр {0} не может быть больше параметра {1}.";
    private const string PageOutOfRangeException = "Номер страницы должен быть больше ноля.";
    private const string PageSizeOutOfRangeException = "Размер страницы должен быть больше ноля.";
    private const string EventNotFoundException = "Событие не найдено";

    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;

    public async Task<PaginatedResult<Event>> GetEventsAsync(string? title, DateTime? from, DateTime? to,
        int page = DefaultPage, int pageSize = DefaultPageSize)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new BadRequestException(string.Format(DateOutOfRangeException, nameof(from), nameof(to)));

        if (page < 1)
            throw new BadRequestException(PageOutOfRangeException);

        if (pageSize < 1)
            throw new BadRequestException(PageSizeOutOfRangeException);

        return await eventRepository.GetEventsAsync(title, from, to, page, pageSize);
    }

    public async Task<Event> GetEventAsync(Guid id)
    {
        var cacheKey = Constants.EventCacheKey(id);

        var cached = await cacheService.Get<Event>(cacheKey);

        if (cached != null)
        {
            return cached;
        }

        var @event = await eventRepository.GetEventAsync(id) ?? throw new NotFoundException(EventNotFoundException);
        await cacheService.Set(cacheKey, @event, TimeSpan.FromMinutes(1));
        return @event;
    }

    public async Task<bool> AddEventAsync(Event @event)
    {
        if (await eventRepository.GetEventAsync(@event.Id) != null)
            return false;

        await eventRepository.CreateEventAsync(@event);
        return await eventRepository.SaveChangesAsync() > 0;
    }

    public async Task<IList<Event>> GetTop10EventsAsync()
    {
        const string cacheKey = Constants.Top10EventsCacheKey;

        var cached = await cacheService.Get<List<Event>>(cacheKey);

        if (cached != null)
        {
            return cached;
        }

        var events = await eventRepository.GetTop10EventsAsync();
        await cacheService.Set(cacheKey, events, TimeSpan.FromMinutes(10));
        return events;
    }

    public async Task ChangeEventAsync(Event @event)
    {
        var entity = await eventRepository.GetEventAsync(@event.Id);
        if (entity == null)
            throw new NotFoundException(EventNotFoundException);

        entity.Update(@event.Title, @event.Description, @event.StartAt, @event.EndAt, @event.TotalSeats);
        await eventRepository.SaveChangesAsync();
        
        var cacheKey = Constants.EventCacheKey(@event.Id);
        await cacheService.Remove(cacheKey);
    }

    public async Task RemoveEventAsync(Guid id)
    {
        if (!await eventRepository.RemoveEventAsync(id))
            throw new NotFoundException(EventNotFoundException);

        await eventRepository.SaveChangesAsync();
        
        var cacheKey = Constants.EventCacheKey(id);
        await cacheService.Remove(cacheKey);
    }
}