using System.Text.Json;
using Confluent.Kafka;
using EventCalendar.Contracts;
using EventCalendar.Events.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventCalendar.Events.Infrastructure.Services;

public sealed class BookingConfirmedConsumer(
    KafkaSettings settings,
    IServiceScopeFactory scopeFactory,
    ILogger<BookingConfirmedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        }).Build();

        consumer.Subscribe(BookingTopics.BookingConfirmed);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = await Task.Run(() => consumer.Consume(stoppingToken), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (KafkaException exception)
                {
                    logger.LogError(exception, "Could not read Kafka topic {Topic}", BookingTopics.BookingConfirmed);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                try
                {
                    await ProcessMessageAsync(result, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Could not process Kafka message at {Offset}", result.TopicPartitionOffset);
                    consumer.Seek(result.TopicPartitionOffset);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                try
                {
                    consumer.Commit(result);
                }
                catch (KafkaException exception)
                {
                    logger.LogError(exception, "Could not commit Kafka message at {Offset}", result.TopicPartitionOffset);
                    consumer.Seek(result.TopicPartitionOffset);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(result.Message.Value))
        {
            logger.LogWarning("Empty booking confirmation at {Offset}", result.TopicPartitionOffset);
            return;
        }

        BookingConfirmed? message;
        try
        {
            message = JsonSerializer.Deserialize<BookingConfirmed>(result.Message.Value);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Invalid booking confirmation at {Offset}", result.TopicPartitionOffset);
            return;
        }

        if (message is null)
        {
            logger.LogWarning("Empty booking confirmation at {Offset}", result.TopicPartitionOffset);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IBookingConfirmationHandler>();
        var outcome = await handler.HandleAsync(message, cancellationToken);

        switch (outcome)
        {
            case BookingConfirmationResult.Reserved:
                logger.LogInformation("Reserved {SeatCount} seats for booking {BookingId}",
                    message.SeatCount, message.BookingId);
                break;
            case BookingConfirmationResult.EventNotFound:
                logger.LogWarning("Event {EventId} for booking {BookingId} not found; skipping message",
                    message.EventId, message.BookingId);
                break;
            case BookingConfirmationResult.NoAvailableSeats:
                logger.LogWarning("Event {EventId} has no seats for booking {BookingId}; skipping message",
                    message.EventId, message.BookingId);
                break;
            case BookingConfirmationResult.InvalidMessage:
                logger.LogWarning("Invalid booking confirmation at {Offset}; skipping message",
                    result.TopicPartitionOffset);
                break;
        }
    }
}
