using System.ComponentModel.DataAnnotations;

namespace EventCalendar.Controllers.Dtos;

public record EventDto : IValidatableObject
{
    [Required(ErrorMessage = "Название события является обязательным.")]
    public required string Title { get; init; }

    public string? Description { get; init; }

    [Required(ErrorMessage = "Дата и время начала события являются обязательными.")]
    public DateTime StartAt { get; init; }

    [Required(ErrorMessage = "Дата и время конца события являются обязательными.")]
    public DateTime EndAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt >= EndAt)
        {
            yield return new ValidationResult(
                "Дата и время начала должны быть меньше даты и времени конца.",
                [nameof(StartAt), nameof(EndAt)]);
        }
    }
}