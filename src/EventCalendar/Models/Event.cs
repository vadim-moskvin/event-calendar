using System.ComponentModel.DataAnnotations;

namespace EventCalendar.Models;

public class Event
{
    private Event()
    {
    }

    public Event(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
    {
        Validate(title);
        Validate(startAt, endAt);
        ValidateTotalSeats(totalSeats);

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        AvailableSeats = TotalSeats = totalSeats;
    }

    public Guid Id { get; }

    public string Title { get; private set; } = null!;

    public string? Description { get; set; }

    public DateTime StartAt { get; private set; }

    public DateTime EndAt { get; private set; }

    public int TotalSeats { get; init; }

    public int AvailableSeats { get; private set; }

    public List<Booking> Bookings { get; private set; } = null!;

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Validate(title);
        Validate(startAt, endAt);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
            return false;

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats += count;
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

    private static void ValidateTotalSeats(int totalSeats)
    {
        if (totalSeats < 1)
            throw new ValidationException("Число мест должно быть больше ноля");
    }
}