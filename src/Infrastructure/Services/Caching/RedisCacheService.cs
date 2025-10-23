using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using Getir.Application.Common;

namespace Getir.Infrastructure.Services.Caching;

/// <summary>
/// Production-ready Redis cache implementation with circuit breaker, retry logic, and fallback
/// Follows global standards and best practices
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ICacheService _fallbackCache;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _isRedisAvailable;
    private DateTime _lastConnectionCheck;
    private readonly TimeSpan _connectionCheckInterval = TimeSpan.FromMinutes(1);
    private readonly SemaphoreSlim _connectionCheckLock = new(1, 1);

    /// <summary>
    /// Constructor with dependency injection
    /// Uses MemoryCacheService as fallback when Redis is unavailable
    /// </summary>
    public RedisCacheService(
        IConnectionMultiplexer? redis,
        ILogger<RedisCacheService> logger,
        ICacheService fallbackCache)
    {
        _redis = redis;
        _logger = logger;
        _fallbackCache = fallbackCache;
        
        // JSON serialization options for performance
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false, // Compact JSON for better performance
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Initialize Redis connection
        if (_redis != null && _redis.IsConnected)
        {
            try
            {
                _database = _redis.GetDatabase();
                _isRedisAvailable = true;
                _logger.LogInformation("Redis cache initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Redis database, falling back to memory cache");
                _isRedisAvailable = false;
            }
        }
        else
        {
            _logger.LogWarning("Redis connection not available, using fallback memory cache");
            _isRedisAvailable = false;
        }
    }

    #region ICacheService Implementation

    /// <summary>
    /// Cache'den değer getir
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>Cache değeri veya null</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            // Check Redis availability with circuit breaker pattern
            if (!await IsRedisAvailableAsync())
            {
                _logger.LogDebug("Redis unavailable, using fallback cache for GET: {Key}", key);
                return await _fallbackCache.GetAsync<T>(key, cancellationToken);
            }

            // Try to get from Redis
            var value = await _database!.StringGetAsync(key);
            
            if (!value.HasValue)
            {
                _logger.LogDebug("Cache MISS: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache HIT: {Key}", key);
            
            // Deserialize JSON to object
            var deserializedValue = JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
            return deserializedValue;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection error on GET, falling back to memory cache: {Key}", key);
            _isRedisAvailable = false;
            return await _fallbackCache.GetAsync<T>(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Cache'e değer set
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Cache değeri</param>
    /// <param name="expiration">Cache süresi</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>İşlem başarı durumu</returns>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            // Always set to fallback cache for double-layer caching
            await _fallbackCache.SetAsync(key, value, expiration, cancellationToken);

            // Check Redis availability
            if (!await IsRedisAvailableAsync())
            {
                _logger.LogDebug("Redis unavailable, value only set in fallback cache: {Key}", key);
                return;
            }

            // Serialize to JSON
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            
            // Set to Redis with expiration
            var ttl = expiration ?? TimeSpan.FromMinutes(30); // Default 30 minutes
            await _database!.StringSetAsync(key, serializedValue, ttl);
            
            _logger.LogDebug("Cache SET: {Key} with TTL: {TTL} seconds", key, ttl.TotalSeconds);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection error on SET, value saved in fallback cache only: {Key}", key);
            _isRedisAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Cache'den değer sil
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>İşlem başarı durumu</returns>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            // Remove from fallback cache
            await _fallbackCache.RemoveAsync(key, cancellationToken);

            // Check Redis availability
            if (!await IsRedisAvailableAsync())
            {
                return;
            }

            // Remove from Redis
            await _database!.KeyDeleteAsync(key);
            _logger.LogDebug("Cache REMOVE: {Key}", key);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection error on REMOVE: {Key}", key);
            _isRedisAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Cache'den pattern'e göre değerleri sil
    /// </summary>
    /// <param name="pattern">Cache pattern</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>İşlem başarı durumu</returns>
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        try
        {
            // Remove from fallback cache (limited support)
            await _fallbackCache.RemoveByPatternAsync(pattern, cancellationToken);

            // Check Redis availability
            if (!await IsRedisAvailableAsync())
            {
                return;
            }

            // Get all servers (for cluster support)
            var endpoints = _redis!.GetEndPoints();
            var tasks = new List<Task>();

            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                
                // Skip replica servers
                if (server.IsReplica)
                    continue;

                // Scan for keys matching pattern (SCAN is better than KEYS for production)
                var keys = server.KeysAsync(pattern: pattern, pageSize: 1000);
                
                tasks.Add(Task.Run(async () =>
                {
                    await foreach (var key in keys)
                    {
                        await _database!.KeyDeleteAsync(key);
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
            _logger.LogInformation("Cache pattern REMOVE: {Pattern}", pattern);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection error on pattern REMOVE: {Pattern}", pattern);
            _isRedisAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Cache'de değer var mı kontrol et
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>Cache değeri var mı</returns>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            // Check Redis availability
            if (!await IsRedisAvailableAsync())
            {
                return await _fallbackCache.ExistsAsync(key, cancellationToken);
            }

            var exists = await _database!.KeyExistsAsync(key);
            _logger.LogDebug("Cache EXISTS: {Key} = {Exists}", key, exists);
            return exists;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection error on EXISTS, using fallback: {Key}", key);
            _isRedisAvailable = false;
            return await _fallbackCache.ExistsAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Cache'i temizle
    /// </summary>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>İşlem başarı durumu</returns>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Clear fallback cache
            await _fallbackCache.ClearAsync(cancellationToken);

            // Check Redis availability
            if (!await IsRedisAvailableAsync())
            {
                return;
            }

            // Flush all databases (use with caution in production!)
            var endpoints = _redis!.GetEndPoints();
            
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                
                // Skip replica servers
                if (server.IsReplica)
                    continue;

                await server.FlushDatabaseAsync();
            }

            _logger.LogWarning("Cache CLEARED - all data flushed!");
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection error on CLEAR");
            _isRedisAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }

    #endregion

    #region Circuit Breaker Pattern

    /// <summary>
    /// Checks if Redis is available with circuit breaker pattern
    /// Prevents continuous connection attempts when Redis is down
    /// </summary>
    /// <returns>Redis bağlantısı var mı</returns>
    private async Task<bool> IsRedisAvailableAsync()
    {
        // If Redis was never initialized, return false
        if (_redis == null || _database == null)
        {
            return false;
        }

        // If Redis is known to be available and connection is active
        if (_isRedisAvailable && _redis.IsConnected)
        {
            return true;
        }

        // Check if enough time has passed since last connection check
        if (DateTime.UtcNow - _lastConnectionCheck < _connectionCheckInterval)
        {
            return false;
        }

        // Try to reconnect (with lock to prevent multiple simultaneous attempts)
        await _connectionCheckLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (DateTime.UtcNow - _lastConnectionCheck < _connectionCheckInterval)
            {
                return _isRedisAvailable;
            }

            _lastConnectionCheck = DateTime.UtcNow;

            // Try a simple PING command
            var pong = await _database.PingAsync();
            
            if (pong.TotalMilliseconds < 1000) // Less than 1 second
            {
                _isRedisAvailable = true;
                _logger.LogInformation("Redis connection restored. Ping: {PingMs}ms", pong.TotalMilliseconds);
                return true;
            }
            else
            {
                _logger.LogWarning("Redis ping too slow: {PingMs}ms", pong.TotalMilliseconds);
                _isRedisAvailable = false;
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis connection check failed");
            _isRedisAvailable = false;
            return false;
        }
        finally
        {
            _connectionCheckLock.Release();
        }
    }

    #endregion

    #region Advanced Redis Operations (Bonus)

    /// <summary>
    /// Gets multiple keys at once (pipeline optimization)
    /// </summary>
    /// <param name="keys">Cache key listesi</param>
    /// <param name="cancellationToken">İptal token</param>
    /// <returns>Cache değerleri</returns>
    public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class
    {
        var result = new Dictionary<string, T?>();

        try
        {
            if (!await IsRedisAvailableAsync())
            {
                foreach (var key in keys)
                {
                    result[key] = await _fallbackCache.GetAsync<T>(key, cancellationToken);
                }
                return result;
            }

            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _database!.StringGetAsync(redisKeys);

            for (int i = 0; i < redisKeys.Length; i++)
            {
                var key = (string)redisKeys[i]!;
                
                if (values[i].HasValue)
                {
                    result[key] = JsonSerializer.Deserialize<T>(values[i].ToString(), _jsonOptions);
                }
                else
                {
                    result[key] = null;
                }
            }

            _logger.LogDebug("Cache GET_MANY: {Count} keys fetched", keys.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple cache values");
            return result;
        }
    }

    /// <summary>
    /// Increments a counter (useful for rate limiting, analytics)
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Artırılacak değer</param>
    /// <param name="expiration">Cache süresi</param>
    /// <returns>Artırılmış değer</returns>
    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null)
    {
        try
        {
            if (!await IsRedisAvailableAsync())
            {
                return 0;
            }

            var result = await _database!.StringIncrementAsync(key, value);
            
            if (expiration.HasValue)
            {
                await _database.KeyExpireAsync(key, expiration.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cache value for key: {Key}", key);
            return 0;
        }
    }

    #endregion
}

