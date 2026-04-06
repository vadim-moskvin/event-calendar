namespace EventCalendar.Models;

public class Event
{
    public Event(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null)
    {
        Validate(startAt, endAt);

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string? Description { get; set; }

    public DateTime StartAt { get; private set; }

    public DateTime EndAt { get; private set; }

    public void Rename(string title)
    {
        Validate(StartAt, EndAt);
        Title = title;
    }

    public void Reschedule(DateTime startAt, DateTime endAt)
    {
        Validate(startAt, endAt);
        StartAt = startAt;
        EndAt = endAt;
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Validate(title);
        Validate(startAt, endAt);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    private static void Validate(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название события должно быть задано", nameof(title));
    }

    private static void Validate(DateTime startAt, DateTime endAt)
    {
        if (startAt >= endAt)
            throw new ArgumentException("Дата и время начала должны быть меньше даты и времени конца.");
    }
}