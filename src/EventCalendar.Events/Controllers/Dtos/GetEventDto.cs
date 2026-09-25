using System.ComponentModel.DataAnnotations;

namespace EventCalendar.Events.Controllers.Dtos;

public record GetEventDto : EventDto
{
    [Required(ErrorMessage = "Идентификатор должен быть задан.")]
    public Guid Id { get; init; }

    public int AvailableSeats { get; init; }
}