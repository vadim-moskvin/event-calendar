using System.ComponentModel.DataAnnotations;

namespace EventCalendar.Controllers.Dtos;

public record EventDto : IValidatableObject
{
    [Required(ErrorMessage = "Название события является обязательным.")]
    public required string Title { get; init; }

    public string? Description { get; init; }

    public DateTime StartAt { get; init; }

    public DateTime EndAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt == default)
        {
            yield return new ValidationResult(
                "Дата и время начала события является обязательным.",
                [nameof(StartAt)]);
        }

        if (EndAt == default)
        {
            yield return new ValidationResult(
                "Дата и время конца события является обязательным.",
                [nameof(EndAt)]);
        }

        if (StartAt != default && EndAt != default && StartAt >= EndAt)
        {
            yield return new ValidationResult(
                "Дата и время начала должны быть меньше даты и времени конца.",
                [nameof(StartAt), nameof(EndAt)]);
        }
    }
}