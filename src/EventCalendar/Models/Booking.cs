using EventCalendar.Exceptions;

namespace EventCalendar.Models;

public class Booking
{
    private const string PendingStatusMessage =
        $"Можно подтвердить только событие в статусе {nameof(BookingStatus.Pending)}";

    private Booking()
    {
    }

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
    
    public Event Event { get; private set; }

    public static Booking MakeNew(Guid eventId)
    {
        return new Booking(Guid.NewGuid(), eventId, DateTime.UtcNow);
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new BadRequestException(PendingStatusMessage);

        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != BookingStatus.Pending)
            throw new BadRequestException(PendingStatusMessage);

        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }
}