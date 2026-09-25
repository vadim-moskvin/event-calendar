using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Application.Services;
using EventCalendar.Bookings.Infrastructure.DataAccess;
using EventCalendar.Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.Tests.TestHelpers;

public abstract class TestsBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IBookingService BookingService;

    protected TestsBase()
    {
        var services = new ServiceCollection();
        var databaseName = $"BookingsTest_{Guid.NewGuid():N}";
        services.AddDbContext<BookingsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();

        ServiceProvider = services.BuildServiceProvider();
        BookingService = ServiceProvider.GetRequiredService<IBookingService>();
    }

    public void Dispose() => ServiceProvider.Dispose();
}
