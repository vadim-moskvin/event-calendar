namespace EventCalendar.Events.Application.Services;

public interface ICacheService
{
    public Task<T?> Get<T>(string key) where T : class;

    public Task Set<T>(string key, T value, TimeSpan expiration);

    public Task Remove(string key);
}