using EventCalendar.Events.Application.Repositories;
using EventCalendar.Events.Infrastructure.DataAccess;
using EventCalendar.Events.Infrastructure.Repositories;
using EventCalendar.Events.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, KafkaSettings kafkaSettings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(kafkaSettings.BootstrapServers);
        ArgumentException.ThrowIfNullOrWhiteSpace(kafkaSettings.ConsumerGroup);

        services.AddDbContext<EventsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddSingleton(kafkaSettings);
        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();

        return services;
    }
}
