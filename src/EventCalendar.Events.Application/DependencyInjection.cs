using EventCalendar.Events.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Events.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingConfirmationHandler, BookingConfirmationHandler>();
        return services;
    }
}
