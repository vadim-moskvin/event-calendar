using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Infrastructure.DataAccess;
using EventCalendar.Bookings.Infrastructure.Repositories;
using EventCalendar.Bookings.Infrastructure.Services;
using EventCalendar.Bookings.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string bootstrapServers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(bootstrapServers);

        services.AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<IBookingConfirmedPublisher>(
            _ => new KafkaBookingConfirmedPublisher(bootstrapServers));
        services.AddHostedService<BookingProcessor>();

        return services;
    }
}
