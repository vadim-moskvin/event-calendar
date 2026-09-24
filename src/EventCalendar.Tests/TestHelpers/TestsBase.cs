using EventCalendar.Application.Repositories;
using EventCalendar.Application.Services;
using EventCalendar.Infrastructure.DataAccess;
using EventCalendar.Infrastructure.Repositories;
using EventCalendar.Infrastructure.Services;
using EventCalendar.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Tests.TestHelpers;

public abstract class TestsBase
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IEventService EventService;
    protected readonly IBookingService BookingService;
    protected readonly IUserService UserService;

    protected TestsBase()
    {
        var services = new ServiceCollection();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{dbName}"));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.Configure<TokenSettings>(settings =>
        {
            settings.SecretKey = "0123456789abcdef0123456789abcdef";
            settings.Issuer = "test-issuer";
            settings.Audience = "test-audience";
            settings.ExpirationMinutes = 30;
        });

        ServiceProvider = services.BuildServiceProvider();
        EventService = ServiceProvider.GetRequiredService<IEventService>();
        BookingService = ServiceProvider.GetRequiredService<IBookingService>();
        UserService = ServiceProvider.GetRequiredService<IUserService>();
        ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
