using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Infrastructure.DataAccess;
using EventCalendar.Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBookingRepository, BookingRepository>();
        // BookingProcessor включим вместе с отправкой запросов в Events через Kafka.

        return services;
    }
}
