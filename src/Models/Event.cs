using System.ComponentModel.DataAnnotations;

namespace EventCalendar.Models;

public class Event
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }
}