namespace EventCalendar.Controllers.Dtos;

public record CreateEventDto : EventDto
{
    public Guid? Id { get; init; }
}