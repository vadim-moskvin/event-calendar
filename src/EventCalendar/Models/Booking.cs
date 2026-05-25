namespace EventCalendar.Models;

public class Booking
{
    private Booking(Guid id, Guid eventId, DateTime createdAt)
    {
        Id = id;
        EventId = eventId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public Guid EventId { get; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? ProcessedAt { get; private set; }

    public static Booking MakeNew(Guid eventId)
    {
        return new Booking(Guid.NewGuid(), eventId, DateTime.UtcNow);
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }
}