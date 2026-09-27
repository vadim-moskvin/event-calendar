using System.Text.Json;
using EventCalendar.Events.Application.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EventCalendar.Events.Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _db = connection.GetDatabase();

    public async Task<T?> Get<T>(string key) where T : class
    {
        try
        {
            var value = await _db.StringGetAsync(key);

            return !value.HasValue ? null : JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (RedisException ex)
        {
            logger.LogError("Не удалось получить значение из Redis: {ExMessage}", ex.Message);
            return null;
        }
    }

    public async Task Set<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiration);
        }
        catch (RedisException ex)
        {
            logger.LogError("Не удалось добавить значение в Redis: {ExMessage}", ex.Message);
        }
    }

    public async Task Remove(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            logger.LogError("Не удалось удалить значение из Redis: {ExMessage}", ex.Message);
        }
    }
}