using EventCalendar.Auth.Application.Repositories;
using EventCalendar.Auth.Domain.Models;
using EventCalendar.Auth.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Auth.Infrastructure.Repositories;

public class UserRepository(AuthDbContext appDbContext) : IUserRepository
{
    public Task CreateUserAsync(User user)
    {
        appDbContext.Users.Add(user);
        return appDbContext.SaveChangesAsync();
    }

    public Task<User?> FindUserAsync(string login)
    {
        return appDbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }
}
