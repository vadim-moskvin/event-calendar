using EventCalendar.Models;

namespace EventCalendar.Controllers.Dtos;

public static class Mapper
{
    public static GetEventDto ToGetEventDto(this Event @event)
    {
        return new GetEventDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt
        };
    }

    public static Event ToEntity(this EventDto dto, Guid id)
    {
        return new Event(id, dto.Title, dto.StartAt, dto.EndAt)
            { Description = dto.Description };
    }

    public static Event ToEntity(this CreateEventDto dto)
    {
        return new Event(dto.Id ?? Guid.NewGuid(), dto.Title, dto.StartAt, dto.EndAt)
            { Description = dto.Description };
    }
}