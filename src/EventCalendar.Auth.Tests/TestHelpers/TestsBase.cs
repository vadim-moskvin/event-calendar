using EventCalendar.Auth.Application;
using EventCalendar.Auth.Application.Repositories;
using EventCalendar.Auth.Application.Services;
using EventCalendar.Auth.Infrastructure.DataAccess;
using EventCalendar.Auth.Infrastructure.Repositories;
using EventCalendar.Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventCalendar.Auth.Tests.TestHelpers;

public abstract class TestsBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IUserService UserService;

    protected TestsBase()
    {
        var services = new ServiceCollection();
        var databaseName = $"AuthTest_{Guid.NewGuid():N}";
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IOptions<TokenSettings>>(Options.Create(new TokenSettings
        {
            SecretKey = "0123456789abcdef0123456789abcdef",
            Issuer = "test-issuer",
            Audiences = ["EventCalendar.Events", "EventCalendar.Bookings"],
            ExpirationMinutes = 30
        }));

        ServiceProvider = services.BuildServiceProvider();
        UserService = ServiceProvider.GetRequiredService<IUserService>();
    }

    public void Dispose() => ServiceProvider.Dispose();
}
