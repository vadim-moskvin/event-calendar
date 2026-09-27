namespace EventCalendar.Auth.Controllers.Dtos;

public record LoginDto()
{
    public required string Login { get; init; }

    public required string Password { get; init; }
}