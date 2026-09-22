using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public interface ITokenService
{
    string GenerateToken(Guid userId, string login, Role role);
}