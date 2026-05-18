using EventCalendar.Exceptions;
using EventCalendar.Models;

namespace EventCalendar.Services;

public class EventService : IEventService
{
    private const string DateOutOfRangeException = "Параметр {0} не может быть больше параметра {1}.";
    private const string PageOutOfRangeException = "Номер страницы должен быть больше ноля.";
    private const string PageSizeOutOfRangeException = "Размер страницы должен быть больше ноля.";
    private const string EventNotFoundException= "Событие не найдено";

    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;

    private readonly Dictionary<Guid, Event> _events = [];

    public PaginatedResult<Event> GetEvents(string? title, DateTime? from, DateTime? to, int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new BadRequestException(string.Format(DateOutOfRangeException, nameof(from), nameof(to)));

        if (page < 1)
            throw new BadRequestException(PageOutOfRangeException);

        if (pageSize < 1)
            throw new BadRequestException(PageSizeOutOfRangeException);

        var query = _events.Values.AsQueryable();

        if (title != null)
            query = query.Where(e => e.Title.Contains(title, StringComparison.CurrentCultureIgnoreCase));
        if (from.HasValue)
            query = query.Where(e => e.StartAt >= from);
        if (to.HasValue)
            query = query.Where(e => e.EndAt <= to);

        var filtered = query.ToArray();

        var items = query
            .OrderBy(c => c.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var totalPages = (int)Math.Ceiling((double)filtered.Length / pageSize);

        return new PaginatedResult<Event>(items, page, totalPages, filtered.Length);
    }

    public Event GetEvent(Guid id)
    {
        return _events.GetValueOrDefault(id) ?? throw new NotFoundException(EventNotFoundException);
    }

    public bool AddEvent(Event @event)
    {
        return _events.TryAdd(@event.Id, @event);
    }

    public void ChangeEvent(Event @event)
    {
        if (!_events.ContainsKey(@event.Id))
            throw new NotFoundException(EventNotFoundException);

        _events[@event.Id] = @event;
    }

    public void RemoveEvent(Guid id)
    {
        var result = _events.Remove(id);
        if (!result)
            throw new NotFoundException(EventNotFoundException);
    }
}