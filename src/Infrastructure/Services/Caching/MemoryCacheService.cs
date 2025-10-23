using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Getir.Application.Common;

namespace Getir.Infrastructure.Services.Caching;

/// <summary>
/// In-memory cache implementation (development/fallback)
/// Moved from Application to Infrastructure layer (Clean Architecture)
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    /// <summary>
    /// MemoryCacheService constructor
    /// </summary>
    /// <param name="memoryCache">Memory cache instance</param>
    /// <param name="logger">Logger instance</param>
    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Cache'den değer getir
    /// </summary>
    /// <typeparam name="T">Değer tipi</typeparam>
    /// <param name="key">Cache anahtarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Cache değeri veya null</returns>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _memoryCache.TryGetValue(key, out T? value);
            _logger.LogDebug("Memory Cache {Action}: {Key} - Found: {Found}", "GET", key, value != null);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    /// <summary>
    /// Cache'e değer kaydet
    /// </summary>
    /// <typeparam name="T">Değer tipi</typeparam>
    /// <param name="key">Cache anahtarı</param>
    /// <param name="value">Kaydedilecek değer</param>
    /// <param name="expiration">Süre sonu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            _memoryCache.Set(key, value, options);
            _logger.LogDebug("Memory Cache {Action}: {Key} - Expiration: {Expiration}", "SET", key, expiration);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Cache'den değer sil
    /// </summary>
    /// <param name="key">Cache anahtarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Memory Cache {Action}: {Key}", "REMOVE", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Pattern'e göre cache değerlerini sil
    /// </summary>
    /// <param name="pattern">Silinecek pattern</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Memory cache doesn't support pattern-based removal efficiently
        // This is one reason why Redis is better for production
        _logger.LogWarning("Pattern removal not fully supported in memory cache. Pattern: {Pattern}", pattern);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cache'de değer var mı kontrol et
    /// </summary>
    /// <param name="key">Cache anahtarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Değer var mı</returns>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            _logger.LogDebug("Memory Cache {Action}: {Key} - Exists: {Exists}", "EXISTS", key, exists);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Tüm cache'i temizle
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_memoryCache is MemoryCache mc)
            {
                mc.Compact(1.0); // Clear all entries
            }
            _logger.LogInformation("Memory cache cleared");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }
}

