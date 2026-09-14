using EventCalendar.Domain.Exceptions;

namespace EventCalendar.Domain.Models;

public class Booking
{
    private const string PendingStatusMessage =
        $"Можно подтвердить только событие в статусе {nameof(BookingStatus.Pending)}";

    private const string InvalidCancelMessage =
        $"Можно отменить только событие в статусе {nameof(BookingStatus.Pending)} или {nameof(BookingStatus.Confirmed)}";

    private Booking()
    {
    }

    private Booking(Guid id, Guid userId, Guid eventId, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        EventId = eventId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public Guid UserId { get; private set; }

    public Guid EventId { get; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? ProcessedAt { get; private set; }

    public Event Event { get; private set; } = null!; // Navigation Property

    public static Booking MakeNew(Guid userId, Guid eventId)
    {
        return new Booking(Guid.NewGuid(), userId, eventId, DateTime.UtcNow);
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

    public void Cancel()
    {
        if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
            throw new BadRequestException(InvalidCancelMessage);

        Status = BookingStatus.Cancelled;
    }
}