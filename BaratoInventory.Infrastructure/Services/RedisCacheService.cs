using BaratoInventory.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace BaratoInventory.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IDatabase? _db;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        try
        {
            _db = _redis.GetDatabase();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to Redis database. Caching will gracefully degrade to database queries.");
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_db == null) return default;
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.HasValue)
                return JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET operation failed for key: {Key}", key);
        }
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, CancellationToken cancellationToken = default)
    {
        if (_db == null) return;
        try
        {
            var json = JsonSerializer.Serialize(value);
            if (absoluteExpireTime.HasValue) await _db.StringSetAsync(key, json, absoluteExpireTime.Value); else await _db.StringSetAsync(key, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET operation failed for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_db == null) return;
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DELETE operation failed for key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        if (_db == null) return;
        try
        {
            var endPoint = _redis.GetEndPoints().FirstOrDefault();
            if (endPoint != null)
            {
                var server = _redis.GetServer(endPoint);
                var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();
                if (keys.Any())
                {
                    await _db.KeyDeleteAsync(keys);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DELETE BY PREFIX failed for prefix: {PrefixKey}", prefixKey);
        }
    }
}

