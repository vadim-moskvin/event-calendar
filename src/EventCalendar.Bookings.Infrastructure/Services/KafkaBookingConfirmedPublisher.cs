using System.Text.Json;
using Confluent.Kafka;
using EventCalendar.Bookings.Application.Services;
using EventCalendar.Contracts;

namespace EventCalendar.Bookings.Infrastructure.Services;

public sealed class KafkaBookingConfirmedPublisher : IBookingConfirmedPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaBookingConfirmedPublisher(string bootstrapServers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bootstrapServers);

        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            EnableIdempotence = true
        }).Build();
    }

    public async Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default)
    {
        await _producer.ProduceAsync(
            BookingTopics.BookingConfirmed,
            new Message<string, string>
            {
                Key = message.EventId.ToString("D"),
                Value = JsonSerializer.Serialize(message)
            },
            cancellationToken);
    }

    public void Dispose()
    {
        try
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
        }
        finally
        {
            _producer.Dispose();
        }
    }
}
