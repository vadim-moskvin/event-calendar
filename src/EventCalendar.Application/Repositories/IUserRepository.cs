using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Repositories;

public interface IUserRepository
{
    Task CreateUserAsync(User user);

    Task<User?> FindUserAsync(string login);
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}