using EventCalendar.Auth.Domain.Models;

namespace EventCalendar.Auth.Application.Services;

public interface ITokenService
{
    string GenerateToken(Guid userId, string login, Role role);
}