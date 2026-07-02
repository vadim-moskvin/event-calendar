using EventCalendar.Models;

namespace EventCalendar.Services;

public interface IEventStore
{
    IList<Event> Get();

    Event? Get(Guid id);

    bool Add(Event @event);
    
    bool Update(Event @event);
    
    bool Remove(Guid id);
}