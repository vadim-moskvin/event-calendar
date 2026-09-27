using EventCalendar.Auth.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
