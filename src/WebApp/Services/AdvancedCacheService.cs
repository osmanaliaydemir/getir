using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace WebApp.Services;

public interface IAdvancedCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<TimeSpan?> GetExpirationAsync(string key, CancellationToken cancellationToken = default);
    Task SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<long> DecrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    Task SetManyAsync<T>(Dictionary<string, T> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    Task<long> GetCounterAsync(string key, CancellationToken cancellationToken = default);
    Task SetCounterAsync(string key, long value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetHashAsync(string key, CancellationToken cancellationToken = default);
    Task SetHashAsync(string key, Dictionary<string, string> hash, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task SetWithTagsAsync<T>(string key, T value, IEnumerable<string> tags, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
    Task<long> GetMemoryUsageAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

public class AdvancedCacheService : IAdvancedCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<AdvancedCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdvancedCacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        IConnectionMultiplexer? redis,
        ILogger<AdvancedCacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out T? memoryValue))
            {
                _logger.LogDebug("Cache hit (memory) for key: {Key}", key);
                return memoryValue;
            }

            // Try distributed cache
            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);
                _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                _logger.LogDebug("Cache hit (distributed) for key: {Key}", key);
                return deserializedValue;
            }

            // Try Redis if available
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var redisValue = await database.StringGetAsync(key);
                if (redisValue.HasValue)
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(redisValue!, _jsonOptions);
                    _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                    _logger.LogDebug("Cache hit (Redis) for key: {Key}", key);
                    return deserializedValue;
                }
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var cacheExpiration = expiration ?? TimeSpan.FromHours(1);

            // Set in memory cache
            _memoryCache.Set(key, value, cacheExpiration);

            // Set in distributed cache
            var distributedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheExpiration
            };
            await _distributedCache.SetStringAsync(key, serializedValue, distributedOptions, cancellationToken);

            // Set in Redis if available
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                await database.StringSetAsync(key, serializedValue, cacheExpiration);
            }

            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, cacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key, cancellationToken);

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                await database.KeyDeleteAsync(key);
            }

            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis == null)
            {
                _logger.LogWarning("Redis not available, skipping pattern removal: {Pattern}", pattern);
                return;
            }

            var database = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                await database.KeyDeleteAsync(key);
                _memoryCache.Remove(key.ToString());
            }

            _logger.LogDebug("Cache removed for pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out _))
            {
                return true;
            }

            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                return true;
            }

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                return await database.KeyExistsAsync(key);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetExpirationAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var ttl = await database.KeyTimeToLiveAsync(key);
                return ttl;
            }

            // For distributed cache, we can't easily get TTL
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache expiration for key: {Key}", key);
            return null;
        }
    }

    public async Task SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                await database.KeyExpireAsync(key, expiration);
            }

            _logger.LogDebug("Cache expiration set for key: {Key} to {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache expiration for key: {Key}", key);
        }
    }

    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RefreshAsync(key, cancellationToken);

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                await database.KeyTouchAsync(key);
            }

            _logger.LogDebug("Cache refreshed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var result = await database.StringIncrementAsync(key, value);
                
                if (expiration.HasValue)
                {
                    await database.KeyExpireAsync(key, expiration.Value);
                }

                return result;
            }

            // Fallback to distributed cache
            var currentValue = await GetCounterAsync(key, cancellationToken);
            var newValue = currentValue + value;
            await SetCounterAsync(key, newValue, expiration, cancellationToken);
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cache counter for key: {Key}", key);
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var result = await database.StringDecrementAsync(key, value);
                
                if (expiration.HasValue)
                {
                    await database.KeyExpireAsync(key, expiration.Value);
                }

                return result;
            }

            // Fallback to distributed cache
            var currentValue = await GetCounterAsync(key, cancellationToken);
            var newValue = currentValue - value;
            await SetCounterAsync(key, newValue, expiration, cancellationToken);
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing cache counter for key: {Key}", key);
            return 0;
        }
    }

    public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T?>();
        
        try
        {
            foreach (var key in keys)
            {
                var value = await GetAsync<T>(key, cancellationToken);
                result[key] = value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple cache values");
        }

        return result;
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var kvp in values)
            {
                await SetAsync(kvp.Key, kvp.Value, expiration, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple cache values");
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing multiple cache values");
        }
    }

    public async Task<long> GetCounterAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var value = await database.StringGetAsync(key);
                return value.HasValue ? (long)value : 0;
            }

            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            return long.TryParse(distributedValue, out var result) ? result : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache counter for key: {Key}", key);
            return 0;
        }
    }

    public async Task SetCounterAsync(string key, long value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheExpiration = expiration ?? TimeSpan.FromHours(1);

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                await database.StringSetAsync(key, value, cacheExpiration);
            }
            else
            {
                var distributedOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration
                };
                await _distributedCache.SetStringAsync(key, value.ToString(), distributedOptions, cancellationToken);
            }

            _logger.LogDebug("Cache counter set for key: {Key} to {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache counter for key: {Key}", key);
        }
    }

    public async Task<Dictionary<string, string>> GetHashAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var hash = await database.HashGetAllAsync(key);
                return hash.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
            }

            // Fallback to distributed cache
            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(distributedValue, _jsonOptions) ?? new();
            }

            return new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache hash for key: {Key}", key);
            return new Dictionary<string, string>();
        }
    }

    public async Task SetHashAsync(string key, Dictionary<string, string> hash, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheExpiration = expiration ?? TimeSpan.FromHours(1);

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var hashEntries = hash.Select(x => new HashEntry(x.Key, x.Value)).ToArray();
                await database.HashSetAsync(key, hashEntries);
                await database.KeyExpireAsync(key, cacheExpiration);
            }
            else
            {
                var serializedValue = JsonSerializer.Serialize(hash, _jsonOptions);
                var distributedOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration
                };
                await _distributedCache.SetStringAsync(key, serializedValue, distributedOptions, cancellationToken);
            }

            _logger.LogDebug("Cache hash set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache hash for key: {Key}", key);
        }
    }

    public async Task SetWithTagsAsync<T>(string key, T value, IEnumerable<string> tags, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await SetAsync(key, value, expiration, cancellationToken);

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                foreach (var tag in tags)
                {
                    await database.SetAddAsync($"tag:{tag}", key);
                }
            }

            _logger.LogDebug("Cache set with tags for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache with tags for key: {Key}", key);
        }
    }

    public async Task RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis == null)
            {
                _logger.LogWarning("Redis not available, skipping tag-based removal");
                return;
            }

            var database = _redis.GetDatabase();
            foreach (var tag in tags)
            {
                var keys = await database.SetMembersAsync($"tag:{tag}");
                foreach (var key in keys)
                {
                    await database.KeyDeleteAsync(key.ToString());
                    _memoryCache.Remove(key.ToString());
                }
                await database.KeyDeleteAsync($"tag:{tag}");
            }

            _logger.LogDebug("Cache removed by tags: {Tags}", string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by tags");
        }
    }

    public async Task<long> GetMemoryUsageAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var info = await database.ExecuteAsync("MEMORY", "USAGE", "0");
                return info.IsNull ? 0 : (long)info;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memory usage");
            return 0;
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_memoryCache is MemoryCache mc)
            {
                mc.Compact(1.0);
            }

            if (_redis != null)
            {
                var database = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                await server.FlushDatabaseAsync();
            }

            _logger.LogDebug("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }
}