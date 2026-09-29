namespace EventCalendar.Events.Application;

public static class Constants
{
    public static string EventCacheKey(Guid id) => $"event:{id}";

    public const string Top10EventsCacheKey = "event:top10";
}