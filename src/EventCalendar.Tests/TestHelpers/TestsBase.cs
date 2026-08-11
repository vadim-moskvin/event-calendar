using EventCalendar.DataAccess;
using EventCalendar.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Tests.TestHelpers;

public abstract class TestsBase
{
    protected readonly AppDbContext DbContext;
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IEventService EventService;
    protected readonly IBookingService BookingService;

    protected TestsBase()
    {
        var services = new ServiceCollection();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{dbName}"));

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        ServiceProvider = services.BuildServiceProvider();
        EventService = ServiceProvider.GetRequiredService<IEventService>();
        BookingService = ServiceProvider.GetRequiredService<IBookingService>();
        DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
    }
}