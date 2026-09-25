using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public interface IUserService
{
    public Task<User> Register(string login, string password, Role role);

    public Task<string> Login(string login, string password);
}