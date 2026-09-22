using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Models;
using EventCalendar.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Infrastructure.Repositories;

public class UserRepository(AppDbContext appDbContext) : IUserRepository
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