namespace EventCalendar.Events.Application;

public sealed class CacheSettings
{
    public int GetEventTtlInMinutes { get; init; }

    public int GetTop10EventsTtlInMinutes { get; init; }

    public void Validate()
    {
        if (GetEventTtlInMinutes <= 0)
            throw new InvalidOperationException("CacheSettings:GetEventTtlInMinutes должен быть больше ноля.");

        if (GetTop10EventsTtlInMinutes <= 0)
            throw new InvalidOperationException("CacheSettings:GetTop10EventsTtlInMinutes должен быть больше ноля.");
    }
}