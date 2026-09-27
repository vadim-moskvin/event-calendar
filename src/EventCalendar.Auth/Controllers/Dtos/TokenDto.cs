namespace EventCalendar.Auth.Controllers.Dtos;

public record TokenDto
{
    public required string Token { get; init; }
}