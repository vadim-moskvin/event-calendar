using EventCalendar.Events.Application.Repositories;
using EventCalendar.Events.Application.Services;
using EventCalendar.Events.Infrastructure.DataAccess;
using EventCalendar.Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Events.Tests.TestHelpers;

public abstract class TestsBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IEventService EventService;

    protected TestsBase()
    {
        var services = new ServiceCollection();
        var databaseName = $"EventsTest_{Guid.NewGuid():N}";
        services.AddDbContext<EventsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        ServiceProvider = services.BuildServiceProvider();
        EventService = ServiceProvider.GetRequiredService<IEventService>();
    }

    public void Dispose() => ServiceProvider.Dispose();
}
