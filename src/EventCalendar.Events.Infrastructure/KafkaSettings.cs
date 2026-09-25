namespace EventCalendar.Events.Infrastructure;

public sealed record KafkaSettings(string BootstrapServers, string ConsumerGroup);
