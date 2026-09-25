using EventCalendar.Auth.Application.Repositories;
using EventCalendar.Auth.Application.Services;
using EventCalendar.Auth.Infrastructure.DataAccess;
using EventCalendar.Auth.Infrastructure.Repositories;
using EventCalendar.Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
