using EventCalendar.Models;

namespace EventCalendar.Services;

public class EventService : IEventService
{
    private const string DateOutOfRangeException = "Параметр {0} не может быть больше параметра {1}.";
    private const string PageOutOfRangeException = "Номер страницы должен быть больше ноля.";
    private const string PageSizeOutOfRangeException = "Размер страницы должен быть больше ноля.";

    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;

    private readonly Dictionary<Guid, Event> _events = [];

    public PaginatedResult<Event> GetEvents(string? title, DateTime? from, DateTime? to, int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentOutOfRangeException(nameof(from),
                string.Format(DateOutOfRangeException, nameof(from), nameof(to)));

        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), PageOutOfRangeException);

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), PageSizeOutOfRangeException);

        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

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

    public Event? GetEvent(Guid id)
    {
        return _events.GetValueOrDefault(id);
    }

    public bool AddEvent(Event @event)
    {
        return _events.TryAdd(@event.Id, @event);
    }

    public bool ChangeEvent(Event @event)
    {
        if (!_events.ContainsKey(@event.Id))
            return false;

        _events[@event.Id] = @event;
        return true;
    }

    public bool RemoveEvent(Guid id)
    {
        return _events.Remove(id);
    }
}