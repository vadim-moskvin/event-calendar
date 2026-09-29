using EventCalendar.Events.Application.Services;

namespace EventCalendar.Events.Tests.TestHelpers;

public sealed class StubCacheService : ICacheService
{
    private readonly Dictionary<string, object> _values = new();

    public List<(string Key, object Value, TimeSpan Expiration)> Sets { get; } = [];
    public List<string> RemovedKeys { get; } = [];

    public Task<T?> Get<T>(string key) where T : class =>
        Task.FromResult(_values.TryGetValue(key, out var value) ? (T)value : null);

    public Task Set<T>(string key, T value, TimeSpan expiration)
    {
        _values[key] = value!;
        Sets.Add((key, value!, expiration));
        return Task.CompletedTask;
    }

    public Task Remove(string key)
    {
        _values.Remove(key);
        RemovedKeys.Add(key);
        return Task.CompletedTask;
    }
}
