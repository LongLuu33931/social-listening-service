using System.Text.Json;
using Coka.Social.Listening.Infra.Data;
using StackExchange.Redis;

namespace Coka.Social.Listening.Infra.Helpers;

public class RedisHelper
{
    private readonly IDatabase _db;

    public RedisHelper(RedisConnectionFactory factory)
    {
        _db = factory.GetDatabase();
    }

    /// <summary>
    /// Get a value from Redis and deserialize it
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return default;
        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    /// <summary>
    /// Get raw string value from Redis
    /// </summary>
    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    /// <summary>
    /// Set a value in Redis with optional TTL
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var json = JsonSerializer.Serialize(value);
        if (ttl.HasValue)
            await _db.StringSetAsync(key, json, ttl.Value);
        else
            await _db.StringSetAsync(key, json);
    }

    /// <summary>
    /// Set a raw string value in Redis with optional TTL
    /// </summary>
    public async Task SetStringAsync(string key, string value, TimeSpan? ttl = null)
    {
        if (ttl.HasValue)
            await _db.StringSetAsync(key, value, ttl.Value);
        else
            await _db.StringSetAsync(key, value);
    }

    /// <summary>
    /// Delete a key from Redis
    /// </summary>
    public async Task<bool> DeleteAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Check if a key exists in Redis
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    /// <summary>
    /// Set TTL on an existing key
    /// </summary>
    public async Task<bool> SetExpiryAsync(string key, TimeSpan ttl)
    {
        return await _db.KeyExpireAsync(key, ttl);
    }
}
