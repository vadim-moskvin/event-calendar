using EventCalendar.Auth.Domain.Models;

namespace EventCalendar.Auth.Application.Repositories;

public interface IUserRepository
{
    Task CreateUserAsync(User user);

    Task<User?> FindUserAsync(string login);
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}