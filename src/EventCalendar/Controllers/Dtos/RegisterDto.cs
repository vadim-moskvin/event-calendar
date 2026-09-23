using System.ComponentModel.DataAnnotations;
using EventCalendar.Domain.Models;

namespace EventCalendar.Controllers.Dtos;

public record RegisterDto
{
    [Required] public required string Login { get; init; }

    [Required] public required string Password { get; init; }

    [Required] public required Role Role { get; init; } = Role.User;
}