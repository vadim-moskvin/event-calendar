using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventCalendar.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventCalendar.Events.Infrastructure.Services;

public sealed class KafkaTopicInitializer(
    KafkaSettings settings,
    ILogger<KafkaTopicInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = settings.BootstrapServers
            }).Build();

            await admin.CreateTopicsAsync(
                [new TopicSpecification
                {
                    Name = BookingTopics.BookingConfirmed,
                    NumPartitions = 3,
                    ReplicationFactor = 1
                }],
                new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(5) })
                .WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);

            logger.LogInformation("Kafka topic {Topic} created", BookingTopics.BookingConfirmed);
        }
        catch (CreateTopicsException exception) when (
            exception.Results.All(result => result.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            logger.LogInformation("Kafka topic {Topic} already exists", BookingTopics.BookingConfirmed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not create Kafka topic {Topic}", BookingTopics.BookingConfirmed);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
