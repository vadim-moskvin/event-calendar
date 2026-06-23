using EventCalendar.Models;

namespace EventCalendar.Controllers.Dtos;

public static class Mapper
{
    public static PaginatedResult<GetEventDto> ToGetEventDto(this PaginatedResult<Event> result)
    {
        return new PaginatedResult<GetEventDto>(result.Items.Select(x => x.ToGetEventDto()), result.CurrentPage,
            result.TotalPages, result.TotalItems);
    }

    public static GetEventDto ToGetEventDto(this Event @event)
    {
        return new GetEventDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.AvailableSeats
        };
    }

    public static Event ToEntity(this EventDto dto, Guid id)
    {
        return new Event(id, dto.Title, dto.StartAt, dto.EndAt, dto.TotalSeats)
            { Description = dto.Description };
    }

    public static Event ToEntity(this EventDto dto)
    {
        return new Event(Guid.NewGuid(), dto.Title, dto.StartAt, dto.EndAt, dto.TotalSeats)
            { Description = dto.Description };
    }

    public static BookingDto ToDto(this Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
    }
}