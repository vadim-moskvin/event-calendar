using EventCalendar.DataAccess;
using EventCalendar.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Tests.TestHelpers;

public abstract class TestsBase
{
    protected readonly IEventService EventService;
    protected readonly IBookingService BookingService;

    protected TestsBase()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        EventService = serviceProvider.GetRequiredService<IEventService>();
        BookingService = serviceProvider.GetRequiredService<IBookingService>();
    }
}