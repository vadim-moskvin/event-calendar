using EventCalendar.Models;

namespace EventCalendar.Controllers.Dtos;

public class BookingDto
{
    public required Guid Id { get; init; }

    public required Guid EventId { get; init; }

    public required string Status { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? ProcessedAt { get; init; }
}