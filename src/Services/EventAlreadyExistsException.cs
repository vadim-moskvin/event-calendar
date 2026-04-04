namespace EventCalendar.Services;

public class EventAlreadyExistsException(Guid id) : Exception($"Event with Id '{id}' already exists.")
{
    public Guid EventId { get; } = id;
}