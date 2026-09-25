using EventCalendar.Bookings.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingProcessingService, BookingProcessingService>();
        return services;
    }
}
