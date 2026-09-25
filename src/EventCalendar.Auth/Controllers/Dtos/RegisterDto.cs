using System.ComponentModel.DataAnnotations;
using EventCalendar.Auth.Domain.Models;

namespace EventCalendar.Auth.Controllers.Dtos;

public record RegisterDto
{
    [Required] public required string Login { get; init; }

    [Required] public required string Password { get; init; }

    [Required] public required Role Role { get; init; } = Role.User;
}